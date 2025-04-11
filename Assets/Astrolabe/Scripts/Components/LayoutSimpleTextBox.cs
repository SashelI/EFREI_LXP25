using System.Collections.Generic;
using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutSimpleTextBox : LayoutBaseTextBox
	{
		/// <summary>
		/// Composant TMP_Text d'une TmpInputField
		/// </summary>
		private TMP_Text _tmpText;

		private RectTransform _caretTransform;

		private RectTransform _rectTransformFromText;

		/// <summary>
		/// Texte déjà setté une fois
		/// </summary>
		public bool firstTextSet = false;

		protected override void AwakeOverride()
		{
			inputFieldTransform = transform.FindRecursively("InputField (TMP)");
			rectTransform = inputFieldTransform.GetComponent<RectTransform>();
			tmpInputField = inputFieldTransform.GetComponent<MRTKTMPInputField>();

			var text = inputFieldTransform.FindRecursively("Text");
			textMeshProText = text.GetComponent<TextMeshProUGUI>();
			_rectTransformFromText = text.GetComponent<RectTransform>();

			var placeHolder = inputFieldTransform.FindRecursively("Placeholder");
			textMeshProPlaceholder = placeHolder.GetComponent<TextMeshProUGUI>();

			var frontPlate = transform.FindRecursively("Frontplate");
			frontPlateHighlight = frontPlate?.GetComponent<RawImage>();

			tmpInputField.onValueChanged.AddListener(OnValueChanged);

			SetIsMultiLineEnabled(IsMultiLineEnabled);
			SetPlaceHolderText(PlaceHolderText);
			SetText(Text, true);
			SetColorWithOpacity();

			tmpInputField.onSelect.AddListener((s) =>
			{
				// Permet de positionner LogicalFocusControl.IsFocus=True à la selection
				((VisualSimpleTextBox)VisualElement).InvokeTextSelected();
			});
		}

		/// <summary>
		/// On remonte avertir Astrolabe du changement
		/// </summary>
		/// <param name="text"></param>
		private void OnValueChanged(string text)
		{
			if (VisualElement != null)
			{
				// passe le nouveau text à Visual qui va l'envoyer à LogicalElement (par abonnement à l'event VisualTextBoseBase TextChanged)
				((VisualSimpleTextBox)VisualElement).InvokeTextChanged(text);
			}
		}

		public override string Text
		{
			get => tmpInputField.text;

			set
			{
				if (tmpInputField.text != value)
				{
					// On maj le curseur la première fois à la fin du texte
					SetText(value, !firstTextSet);

					firstTextSet = true;
					KeyboardManager.Instance.IsNewBoxFocused = false;
				}
			}
		}

		private void UpdateLineCountFromInputField()
		{
			if (textMeshProText != null && IsMultiLineEnabled)
			{
				textMeshProText.ForceMeshUpdate();

				var realTextInfo = textMeshProText.GetTextInfo(textMeshProText.text);

				var lines = realTextInfo.lineInfo;

				var tmpLineCount = realTextInfo.lineCount;

				var linesValues = new List<string>();

				var totalCharacters = realTextInfo.characterInfo.Length;

				if (totalCharacters > 0)
				{
					var currentTotalCharacters = 0;

					var realNumberLines = 0;

					for (var i = 0; i < lines.Length; i++)
					{
						if (currentTotalCharacters >= totalCharacters)
						{
							break;
						}

						var line = lines[i];

						var lineValue = string.Empty;

						for (var c = line.firstCharacterIndex; c < line.lastCharacterIndex + 1; c++)
						{
							try
							{
								var charInfo = realTextInfo.characterInfo[c];

								lineValue += charInfo.character;
								currentTotalCharacters++;

								if (currentTotalCharacters >= totalCharacters)
								{
									break;
								}
							}
							catch (System.Exception ex)
							{
								// A cause du système de cache du TMP qui est complètement claqué, quand on fait une suppression,
								// il essaie de rechercher des caractères en fonction des infos de ligne dans un texte qui n'existe plus
								Log.WriteLine(ex);
								break;
							}
						}

						if (!string.IsNullOrEmpty(lineValue))
						{
							linesValues.Add(lineValue);
							realNumberLines++;
						}
					}
				}

				// On envoie les lignes à Core pour qu'il gère la navigation à partir de cette info
				((VisualSimpleTextBox)VisualElement).InvokeVisualLinesChanged(linesValues);
			}
		}

		private void SetText(string value, bool moveCaretToEnd)
		{
			if (tmpInputField != null)
			{
				tmpInputField.text = value;
				if (moveCaretToEnd)
				{
					MoveCaret(tmpInputField.text?.Length ?? 0);

					//Si jamais on passe d'un grand texte a un petit : remettre la vue du string en place
					var pos = textMeshProText.GetComponent<RectTransform>();
					pos.offsetMin = Vector2.zero;
					pos.offsetMax = Vector2.zero;

					var caret = inputFieldTransform.FindRecursively("Caret");
					if (caret != null)
					{
						_caretTransform = caret.GetComponent<RectTransform>();
						_caretTransform.offsetMin = Vector2.zero;
						_caretTransform.offsetMax = Vector2.zero;
					}
				}

				if (IsMultiLineEnabled)
				{
					UpdateLineCountFromInputField();
				}
			}
		}

		private void SetHorizontalAlignment(HorizontalAlignmentOptions option)
		{
			textMeshProText.alignment = (TextAlignmentOptions)(((int)textMeshProText.alignment & 0xFF00) | (int)option);
		}

		public SolidColorBrush SelectionColor
		{
			get => _selectionColor;

			set
			{
				if (_selectionColor != value)
				{
					_selectionColor = value;

					if (value != null)
					{
						var solidColorBrush = value;

						if (solidColorBrush != null)
						{
							tmpInputField.selectionColor = solidColorBrush.ToUnityColor();
						}
						else
						{
							// si le solidColorBrush est vide
							tmpInputField.selectionColor = new Color32(Colors.MidnightBlue.R, Colors.MidnightBlue.G,
								Colors.MidnightBlue.B, Colors.MidnightBlue.A);
						}
					}
					else
					{
						// si la couleur est vide
						tmpInputField.selectionColor = new Color32(Colors.MidnightBlue.R, Colors.MidnightBlue.G,
							Colors.MidnightBlue.B, Colors.MidnightBlue.A);
					}
				}
			}
		}

		private SolidColorBrush _selectionColor = new(Colors.MidnightBlue);

		/// <summary>
		/// Position du caret
		/// </summary>
		public int CaretPosition
		{
			get => tmpInputField?.caretPosition ?? 0;
			set => MoveCaret(value);
		}

		/// <summary>
		/// Index de la ligne où se trouve le caret
		/// </summary>
		public int CaretLineIndex
		{
			get
			{
				if (tmpInputField != null && textMeshProText != null)
				{
					return textMeshProText.GetTextInfo(textMeshProText.text).characterInfo[tmpInputField.caretPosition]
						.lineNumber;
				}

				return 0;
			}
		}

		/// <summary>
		/// Curseur visible ?
		/// </summary>
		public bool IsCaretVisible { get; set; } = true;

		/// <summary>
		/// Taille du curseur
		/// </summary>
		public int CaretWidth { get; set; } = 5;

		/// <summary>
		/// Gestion manuelle de la sélection du texte ?
		/// (permet de gérer via des actions externes la sélection du texte de l'input)
		/// (et ainsi de ne pas déclencher le onSelect de l'inputfield mrtk côté Foundation)
		/// </summary>
		public bool ManualTextSelectionManagement { get; set; } = false;

		/// <summary>
		/// Selection du text entier
		/// </summary>
		public override void SelectText()
		{
			tmpInputField.shouldHideSoftKeyboard = !ShowSystemKeyboardOnFocus;
			tmpInputField.interactable = true;

			// Permet de sélectionner seulement le texte sur le focus du contrôle
			tmpInputField.onFocusSelectAll = true;
			tmpInputField.caretWidth = 0;

			tmpInputField.ActivateInputField();
		}

		/// <summary>
		/// Selection du texte avec le caret inactif
		/// Appel explicite
		/// </summary>
		public override void DeselectText()
		{
			tmpInputField.shouldHideSoftKeyboard = !ShowSystemKeyboardOnFocus;
			tmpInputField.interactable = true;

			tmpInputField.onFocusSelectAll = false;

			if (IsCaretVisible)
			{
				tmpInputField.caretWidth = CaretWidth;

				// On refix le caret car sinon il le place n'importe ou
				tmpInputField.caretPosition = CaretPosition;
			}
			else
			{
				tmpInputField.caretWidth = 0;
			}

			tmpInputField.ActivateInputField();
		}

		/// <summary>
		/// Déplacer le caret à la position demandée
		/// </summary>
		/// <param name="newPosition"></param>
		public void MoveCaret(int newPosition)
		{
			if (newPosition < 0 || newPosition > tmpInputField.text.Length)
			{
				return;
			}

			if (tmpInputField.interactable && IsCaretVisible)
			{
				if (_rectTransformFromText.anchoredPosition.x < 0)
				{
					// We set the margin left to 0 to avoid the text to be hidden by the negative margin.
					_rectTransformFromText.anchoredPosition = new Vector2(0, _rectTransformFromText.anchoredPosition.y);
				}

				tmpInputField.caretPosition = newPosition;

				UpdateLineCountFromInputField();
			}
		}

		// Ne pas utiliser le LateUpdate(), il affiche le clavier système même si shouldHideSoftKeyboard est à true...
		//private void LateUpdate()
		//{
		//}
	}
}