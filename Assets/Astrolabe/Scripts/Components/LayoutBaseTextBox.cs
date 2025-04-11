using System;
using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutBaseTextBox : TwinkleComponent
	{
		protected RectTransform rectTransform;
		protected MRTKTMPInputField tmpInputField;
		protected TextMeshProUGUI textMeshProText;
		protected TextMeshProUGUI textMeshProPlaceholder;

		protected Transform inputFieldTransform;

		protected RawImage frontPlateHighlight;

		protected BoxCollider textMeshProCollider;

		protected float canvasPixelPerUnit = 1000;
		protected float canvasFontPerUnit = 100;

		protected override void AwakeOverride()
		{
			inputFieldTransform = transform.FindRecursively("InputField (TMP)");
			rectTransform = inputFieldTransform.GetComponent<RectTransform>();
			tmpInputField = inputFieldTransform.GetComponent<MRTKTMPInputField>();

			var text = inputFieldTransform.FindRecursively("Text");
			textMeshProText = text.GetComponent<TextMeshProUGUI>();

			var placeHolder = inputFieldTransform.FindRecursively("Placeholder");
			textMeshProPlaceholder = placeHolder.GetComponent<TextMeshProUGUI>();

			var frontPlate = transform.FindRecursively("Frontplate");
			frontPlateHighlight = frontPlate?.GetComponent<RawImage>();

			tmpInputField.onValueChanged.AddListener(OnValueChanged);

			SetIsMultiLineEnabled(IsMultiLineEnabled);
			SetPlaceHolderText(PlaceHolderText);
			SetText(Text);
			SetColorWithOpacity();

			tmpInputField.onSelect.AddListener((s) =>
			{
				// Permet de positionner LogicalFocusControl.IsFocus=True à la selection
				((VisualBaseTextBox)VisualElement).InvokeTextSelected();

				// necessaire pour que le caret soit bien positionné
				SelectText();
			});
		}

		private bool _isFirstFocused = false;

		/// <summary>
		/// On remonte avertir Astrolabe du changement
		/// </summary>
		/// <param name="text"></param>
		private void OnValueChanged(string text)
		{
			if (VisualElement != null)
			{
				// passe le nouveau text à Visual qui va l'envoyer à LogicalElement (par abonnement à l'event VisualTextBoseBase TextChanged)
				((VisualBaseTextBox)VisualElement).InvokeTextChanged(text);
			}
		}

		protected override void SetWidthOverride(float width)
		{
			if (frontPlateHighlight != null)
			{
				var frontPlateScale = frontPlateHighlight.transform.parent as RectTransform;
				if (frontPlateScale != null)
				{
					frontPlateScale.sizeDelta = new Vector2(width * canvasPixelPerUnit, frontPlateScale.sizeDelta.y);
				}
			}

			var size = rectTransform.sizeDelta;
			rectTransform.sizeDelta = new Vector2(width * canvasPixelPerUnit, size.y);
		}

		protected override void SetHeightOverride(float height)
		{
			if (frontPlateHighlight != null)
			{
				var frontPlateScale = frontPlateHighlight.transform.parent as RectTransform;
				if (frontPlateScale != null)
				{
					frontPlateScale.sizeDelta = new Vector2(frontPlateScale.sizeDelta.x, height * canvasPixelPerUnit);
				}
			}

			var size = rectTransform.sizeDelta;
			rectTransform.sizeDelta = new Vector2(size.x, height * canvasPixelPerUnit);
		}

		protected void SetPlaceHolderText(string value)
		{
			if (textMeshProPlaceholder != null)
			{
				textMeshProPlaceholder.SetText(value);
			}
		}

		public string PlaceHolderText
		{
			get => textMeshProPlaceholder.text;

			set
			{
				if (textMeshProPlaceholder.text != value)
				{
					SetPlaceHolderText(value);
				}
			}
		}

		public virtual string Text
		{
			get => tmpInputField.text;

			set
			{
				if (tmpInputField.text != value)
				{
					SetText(value);
				}
			}
		}

		private void SetText(string value)
		{
			if (tmpInputField != null)
			{
				tmpInputField.text = value;
			}
		}

		public bool IsMultiLineEnabled
		{
			get => _isMultiLineEnabled;

			set
			{
				if (_isMultiLineEnabled != value)
				{
					SetIsMultiLineEnabled(value);
				}
			}
		}

		private bool _isMultiLineEnabled = false;

		protected void SetIsMultiLineEnabled(bool value)
		{
			_isMultiLineEnabled = value;

			if (tmpInputField != null)
			{
				if (value == true)
				{
					textMeshProPlaceholder.verticalAlignment = VerticalAlignmentOptions.Top;
					textMeshProText.verticalAlignment = VerticalAlignmentOptions.Top;
					tmpInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
				}
				else
				{
					textMeshProPlaceholder.verticalAlignment = VerticalAlignmentOptions.Middle;
					textMeshProText.verticalAlignment = VerticalAlignmentOptions.Middle;
					tmpInputField.lineType = TMP_InputField.LineType.SingleLine;
				}
			}
		}

		public float FontSize
		{
			get => _fontSize;

			set
			{
				if (_fontSize != value)
				{
					SetFontSize(value);
				}
			}
		}

		private float _fontSize = 0.014f;

		protected void SetFontSize(float value)
		{
			_fontSize = value;

			if (textMeshProText != null)
			{
				float fs = 0;

				if (float.IsNaN(value))
				{
					fs = 0;
				}
				else
				{
					fs = _fontSize;
				}

				var canvasFs = fs * canvasFontPerUnit;

				textMeshProText.fontSize = canvasFs; // a cause du canvas on multiplie la valeur normal du FontSize
				textMeshProPlaceholder.fontSize = canvasFs;
			}
		}

		public global::Astrolabe.Twinkle.TextAlignment TextAlignment
		{
			get => _textAlignment;

			set
			{
				if (_textAlignment != value)
				{
					SetTextAlignment(value);
				}
			}
		}

		private global::Astrolabe.Twinkle.TextAlignment _textAlignment = global::Astrolabe.Twinkle.TextAlignment.Left;

		protected void SetTextAlignment(global::Astrolabe.Twinkle.TextAlignment value)
		{
			_textAlignment = value;

			if (textMeshProText != null)
			{
				switch (value)
				{
					case global::Astrolabe.Twinkle.TextAlignment.Left:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Left);
						break;

					case global::Astrolabe.Twinkle.TextAlignment.Right:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Right);
						break;

					case global::Astrolabe.Twinkle.TextAlignment.Center:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Center);
						break;

					case global::Astrolabe.Twinkle.TextAlignment.Justify:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Justified);
						break;
				}
			}
		}

		protected void SetHorizontalAlignment(HorizontalAlignmentOptions option)
		{
			textMeshProText.alignment = (TextAlignmentOptions)(((int)textMeshProText.alignment & 0xFF00) | (int)option);
		}

		public float Opacity
		{
			get => _opacity;

			set
			{
				if (_opacity != value)
				{
					_opacity = value;
					SetColorWithOpacity();
				}
			}
		}

		private float _opacity = 1.0f;

		protected void SetColorWithOpacity()
		{
			// ATTENTION un TextBlock avec opacity sur une image utilisant l'opacité ne fonctionnera peut être pas comme il faut (surtout dans les valeur 0.5 à 0 du TextBlock)
			var newColor = _foregroundWhitoutOpacity.GetColorWithOpacity(_opacity);

			textMeshProText.color = newColor;

			// Ci-dessous : provoque un bug quand l'opacity est à 0 et que le parent est Visiblity=Collapsed et qu'on le passe à Opacity=1 puis parent à Visiblity=Visible -> L'affichage de la fonte se casse
			//newColor = outlineColorWhitoutOpacity.GetColorWithOpacity(this.opacity);
			//this.textMeshProText.outlineColor = newColor;
		}

		public CornerRadius CornerRadius
		{
			get
			{
				var radius = frontPlateHighlight.material.GetFloat("_Radius_");
				return new CornerRadius(radius);
			}
			set
			{
				var maxRadiusBottom = Mathf.Max(value.BottomLeft, value.BottomRight);
				var maxRadiusTop = Mathf.Max(value.TopLeft, value.TopRight);
				var maxRadius = Mathf.Max(maxRadiusTop, maxRadiusBottom);

				//On set le radius des highlight au max des radius du rectangle :
				//le shader n'accepte qu'une valeur uniforme donc on prend le coin le plus arrondi
				frontPlateHighlight.material.SetFloat("_Radius_", maxRadius.ToVisual());
			}
		}

		private CornerRadius _cornerRadius;

		public SolidColorBrush Foreground
		{
			get => _foreground;

			set
			{
				if (_foreground != value)
				{
					_foreground = value;

					Color32 color;

					if (value != null)
					{
						var solidColorBrush = value as SolidColorBrush;

						if (solidColorBrush != null)
						{
							color = solidColorBrush.ToUnityColor();
						}
						else
						{
							// si le solidColorBrush est vide
							color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
						}
					}
					else
					{
						// si la couleur est vide
						color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
					}

					// permet de se souvenir de la couleur utilisé
					_foregroundWhitoutOpacity = color;
					SetColorWithOpacity();
				}
			}
		}

		private SolidColorBrush _foreground = new(Colors.White);
		private Color32 _foregroundWhitoutOpacity = new(0xFF, 0xFF, 0xFF, 0xFF);

		public SolidColorBrush PlaceHolderForeground
		{
			get => _placeHolderForeground;

			set
			{
				if (_placeHolderForeground != value)
				{
					_placeHolderForeground = value;

					Color32 color;

					if (value != null)
					{
						var solidColorBrush = value as SolidColorBrush;

						if (solidColorBrush != null)
						{
							color = solidColorBrush.ToUnityColor();
						}
						else
						{
							// si le solidColorBrush est vide
							color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
						}
					}
					else
					{
						// si la couleur est vide
						color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
					}

					textMeshProPlaceholder.color = color;
				}
			}
		}

		public float PlaceHolderFontSize
		{
			get => textMeshProPlaceholder.fontSize;
			set => textMeshProPlaceholder.fontSize = value;
		}

		private SolidColorBrush _placeHolderForeground = new(Colors.White);

		/// <summary>
		/// Doit-on montrer le keyboard système lors du focus ?
		/// </summary>
		public bool ShowSystemKeyboardOnFocus { get; set; } = true;

		public void SetHitTestVisible(bool isHitTestVisible)
		{
			if (isHitTestVisible == true)
			{
				AddCollider();
			}

			if (textMeshProCollider != null)
			{
				textMeshProCollider.enabled = isHitTestVisible;
			}
		}

		private BoxCollider AddCollider()
		{
			if (textMeshProCollider == null)
			{
				var collider = gameObject.GetOrAddComponent<BoxCollider>();

				textMeshProCollider = collider;

				if (collider != null)
				{
					var x = rectTransform.sizeDelta.x;
					var y = rectTransform.sizeDelta.y;

					SetColliderSize(x, y);
				}
			}

			return textMeshProCollider;
		}

		private void SetColliderSize(float width, float height)
		{
			if (textMeshProCollider != null)
			{
				textMeshProCollider.size = new UnityEngine.Vector3(width, height, 0.01f);
			}
		}

		public void SetFontSDF(TMP_FontAsset fontSfd, bool placeholderOnly = false)
		{
			if (fontSfd == null)
			{
				return;
			}

			if (!placeholderOnly)
			{
				textMeshProText.font = fontSfd;
			}

			textMeshProPlaceholder.font = fontSfd;
		}

		public void SetFontStyle(global::Astrolabe.Twinkle.FontStyle fontStyle, global::Astrolabe.Twinkle.FontWeight fontWeight,
			TextDecorations textDecorations, CharacterCasing characterCasing, bool placeholder = false)
		{
			var fontGlobalStyles = FontStyles.Normal;

			if (fontStyle == global::Astrolabe.Twinkle.FontStyle.Italic)
			{
				fontGlobalStyles |= FontStyles.Italic;
			}

			if (fontWeight == global::Astrolabe.Twinkle.FontWeight.Bold)
			{
				fontGlobalStyles |= FontStyles.Bold;
			}

			if ((textDecorations & TextDecorations.Strikethrough) == TextDecorations.Strikethrough)
			{
				fontGlobalStyles |= FontStyles.Strikethrough;
			}

			if ((textDecorations & TextDecorations.Underline) == TextDecorations.Underline)
			{
				fontGlobalStyles |= FontStyles.Underline;
			}

			switch (characterCasing)
			{
				case CharacterCasing.Lower:
					fontGlobalStyles |= FontStyles.LowerCase;
					break;

				case CharacterCasing.Upper:
					fontGlobalStyles |= FontStyles.UpperCase;
					break;
			}

			if (placeholder)
			{
				textMeshProPlaceholder.fontStyle = fontGlobalStyles;
			}
			else
			{
				textMeshProText.fontStyle = fontGlobalStyles;
			}
		}

		/// <summary>
		/// Selection du text avec le caret actif
		/// </summary>
		public virtual void SelectText()
		{
			tmpInputField.shouldHideSoftKeyboard = !ShowSystemKeyboardOnFocus;
			tmpInputField.interactable = true;

			// On refix le caret car sinon il le place n'importe ou
			tmpInputField.caretPosition = Math.Max(0, tmpInputField.text.Length);

			tmpInputField.ActivateInputField();
			tmpInputField.Select();
		}

		/// <summary>
		/// Selection du texte avec le caret inactif
		/// </summary>
		public virtual void DeselectText()
		{
			tmpInputField.shouldHideSoftKeyboard = !ShowSystemKeyboardOnFocus;
			tmpInputField.DeactivateInputField();
			// C'est crado mais j'ai pas trouvé mieux pour virée le caret en cas de unfocus (le deactivate ne fonctionne pas)
			tmpInputField.interactable = false;
		}
	}
}