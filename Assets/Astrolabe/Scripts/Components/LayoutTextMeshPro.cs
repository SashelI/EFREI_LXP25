using Assets.Astrolabe.Scripts.Tools;
using TMPro;
using UnityEngine;
using Twinkle = Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutTextMeshPro : TwinkleComponent
	{
		private RectTransform _rectTransform;
		private BoxCollider _textMeshProCollider;
		private TextMeshPro _textMeshProText;

		/// <summary>
		/// Obtenir la taille du TextMeshPro
		/// </summary>
		/// <returns></returns>
		public Twinkle.Size GetBounds(Twinkle.Size availableSize)
		{
			// permet de contrer le bug d'affichage du trimming character ellipsis en mode taille infini qui s'affiche alors qu'il ne devrait pas.
			// On va le forcer en truncat a la place
			if (float.IsPositiveInfinity(availableSize.Width))
			{
				_textMeshProText.enableWordWrapping = false;
				_textMeshProText.overflowMode = TextOverflowModes.Overflow;
			}
			else
			{
				// ici on a une width de fixé
				var isWrap = _textWrapping == Twinkle.TextWrapping.Wrap;
				_textMeshProText.enableWordWrapping = isWrap;

				switch (_textTrimming)
				{
					case Twinkle.TextTrimming.CharacterEllipsis:
						_textMeshProText.overflowMode = TextOverflowModes.Ellipsis;
						break;

					default:
						_textMeshProText.overflowMode = TextOverflowModes.Truncate;
						break;
				}

				// Height et width sont fixé ?
				if (float.IsPositiveInfinity(availableSize.Height) == false)
				{
					// ici width et height sont fixé (pas besoin de calcul)
					// les overflowmodes sont bien fixés
					return availableSize;
				}
			}

			// il est indispensable de changer le enabled pour forcer le ForceMeshUpdate a avoir la bonne taille
			// en mode ligne et wrap
			//var enabled = textMeshProText.enabled;
			//textMeshProText.enabled = !enabled;
			_textMeshProText.ForceMeshUpdate(true);

			var x = availableSize.Width;
			var y = availableSize.Height;

			// on ne conserve les valeurs que si availableSize est PositiveInfinity
			if (float.IsPositiveInfinity(availableSize.Width) == true)
			{
				// maintenant que le ForceMeshUpdate est OK on peut récupérer la bonne taille de la boite
				var renderSize = _textMeshProText.GetRenderedValues();
				x = renderSize.x;
			}

			if (float.IsPositiveInfinity(availableSize.Height) == true)
			{
				// malheureusement RendererValue ne fonctionne pas bien en hauteur
				// On va donc calculer nous même la hauteur des lignes
				_textMeshProText.GetTextInfo(_text);

				float heightLines = 0;
				var textInfo = _textMeshProText.textInfo;

				for (var i = 0; i < _textMeshProText.textInfo.lineCount; i++)
				{
					heightLines += textInfo.lineInfo[i].lineHeight;
				}

				y = heightLines;
			}

			// On rajoute un peu de taille pour eviter les problèmes du dernier caractère qui n'est pas pris en compte (pas systematique)
			// C'est pas génial mais pour l'instant on ne peut faire que ça
			x += 0.002f;

			return new Twinkle.Size(x, y);
		}

		/// <summary>
		/// Si la taille du TwinkleComponent est Zero on cache le TMP sinon on l'affiche. Par defaut rien n'est affiché pour eviter les glitchs de texte quand aucune taille n'est definie
		/// </summary>
		private void ShowOrHideTextMeshPro()
		{
			if (Width <= 0 || Height <= 0)
			{
				TextMeshPro.localScale = UnityEngine.Vector3.zero;
			}
			else
			{
				TextMeshPro.localScale = UnityEngine.Vector3.one;
			}
		}

		public Twinkle.TextWrapping TextWrapping
		{
			get => TextWrapping;

			set => _textWrapping = value;
			// La prise en compte s'effectue dans le GetBounds
		}

		public Twinkle.SolidColorBrush Foreground
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
						var solidColorBrush = value as Twinkle.SolidColorBrush;

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

		private Twinkle.SolidColorBrush _foreground = new(Twinkle.Colors.White);
		private Color32 _foregroundWhitoutOpacity = new(0xFF, 0xFF, 0xFF, 0xFF);

		public bool IsRichTextBlockEnabled
		{
			get => _textMeshProText.richText;
			set => _textMeshProText.richText = value;
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

		private void SetColorWithOpacity()
		{
			// ATTENTION un TextBlock avec opacity sur une image utilisant l'opacité ne fonctionnera peut être pas comme il faut (surtout dans les valeur 0.5 à 0 du TextBlock)
			var newColor = _foregroundWhitoutOpacity.GetColorWithOpacity(_opacity);
			_textMeshProText.color = newColor;

			// Ci-dessous : provoque un bug quand l'opacity est à 0 et que le parent est Visiblity=Collapsed et qu'on le passe à Opacity=1 puis parent à Visiblity=Visible -> L'affichage de la fonte se casse
			//newColor = outlineColorWhitoutOpacity.GetColorWithOpacity(this.opacity);
			//this.textMeshProText.outlineColor = newColor;
		}

		private Twinkle.TextWrapping _textWrapping = Twinkle.TextWrapping.NoWrap;

		public Twinkle.TextTrimming TextTrimming
		{
			get => TextTrimming;

			set => _textTrimming = value;
		}

		private Twinkle.TextTrimming _textTrimming = Twinkle.TextTrimming.None;

		public Twinkle.TextAlignment HorizontalTextAlignment
		{
			get => _horizontalTextAlignment;

			set
			{
				if (_horizontalTextAlignment != value)
				{
					SetTextAlignment(value);
				}
			}
		}
		private Twinkle.TextAlignment _horizontalTextAlignment = Twinkle.TextAlignment.Left;

		public Twinkle.VerticalAlignment VerticalTextAlignment
		{
			get => _verticalTextAlignment;

			set
			{
				if (_verticalTextAlignment != value)
				{
					VerticalAlignmentOptions alignmentOption = value switch
					{
						Twinkle.VerticalAlignment.Top => VerticalAlignmentOptions.Top,
						Twinkle.VerticalAlignment.Center => VerticalAlignmentOptions.Middle,
						Twinkle.VerticalAlignment.Bottom => VerticalAlignmentOptions.Bottom,
						_ => VerticalAlignmentOptions.Middle
					};

					SetVerticalAlignment(alignmentOption);
				}
			}
		}
		private Twinkle.VerticalAlignment _verticalTextAlignment = Twinkle.VerticalAlignment.Top;

		private void SetTextAlignment(Twinkle.TextAlignment value)
		{
			_horizontalTextAlignment = value;

			if (_textMeshProText != null)
			{
				switch (value)
				{
					case Twinkle.TextAlignment.Left:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Left);
						break;

					case Twinkle.TextAlignment.Right:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Right);
						break;

					case Twinkle.TextAlignment.Center:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Center);
						break;

					case Twinkle.TextAlignment.Justify:
						SetHorizontalAlignment(HorizontalAlignmentOptions.Justified);
						break;
				}
			}
		}

		private void SetHorizontalAlignment(HorizontalAlignmentOptions option)
		{
			_textMeshProText.alignment = (TextAlignmentOptions)(((int)_textMeshProText.alignment & 0xFF00) | (int)option);
		}

		private void SetVerticalAlignment(VerticalAlignmentOptions option)
		{
			_textMeshProText.alignment = (TextAlignmentOptions)(((int)_textMeshProText.alignment & 0x00FF) | (int)option);
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

		private float _fontSize = 0.036f;

		private void SetFontSize(float value)
		{
			_fontSize = value;

			if (_textMeshProText != null)
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

				_textMeshProText.fontSize = fs;
			}
		}

		protected override void SetWidthOverride(float width)
		{
			if (_rectTransform != null)
			{
				_rectTransform.sizeDelta = new UnityEngine.Vector2(width, _rectTransform.sizeDelta.y);
				SetColliderSize(width, _rectTransform.sizeDelta.y);
			}

			ShowOrHideTextMeshPro();
			//this.TextMeshPro.gameObject.SetActive(width != 0);
		}

		protected override void SetHeightOverride(float height)
		{
			if (_rectTransform != null)
			{
				_rectTransform.sizeDelta = new UnityEngine.Vector2(_rectTransform.sizeDelta.x, height);
				SetColliderSize(_rectTransform.sizeDelta.x, height);
			}

			ShowOrHideTextMeshPro();
			//this.TextMeshPro.gameObject.SetActive(height != 0);
		}

		public string Text
		{
			get => _text;

			set
			{
				if (_text != value)
				{
					SetText(value);
				}
			}
		}

		private string _text;

		private void SetText(string value)
		{
			_text = value;

			if (_textMeshProText != null)
			{
				_textMeshProText.SetText(value);
			}
		}

		public void SetFontSDF(TMP_FontAsset fontSfd)
		{
			if (fontSfd == null)
			{
				return;
			}

			//var fontNames = Font.GetPathsToOSFonts();
			//var font = new Font(fontNames[0]);
			//var fontAsset = TMP_FontAsset.CreateFontAsset(font);

			_textMeshProText.font = fontSfd;
			//this.textMeshProText.materialForRendering.shader = Shader.Find("Mixed Reality Toolkit/TextMeshPro");
			//this.SetOutlineThickness(this.outlineThickness);
		}

		public void SetFontStyle(Twinkle.FontStyle fontStyle, Twinkle.FontWeight fontWeight,
			Twinkle.TextDecorations textDecorations, Twinkle.CharacterCasing characterCasing)
		{
			var fontGlobalStyles = FontStyles.Normal;

			if (fontStyle == Twinkle.FontStyle.Italic)
			{
				fontGlobalStyles |= FontStyles.Italic;
			}

			if (fontWeight == Twinkle.FontWeight.Bold)
			{
				fontGlobalStyles |= FontStyles.Bold;
			}

			if ((textDecorations & Twinkle.TextDecorations.Strikethrough) == Twinkle.TextDecorations.Strikethrough)
			{
				fontGlobalStyles |= FontStyles.Strikethrough;
			}

			if ((textDecorations & Twinkle.TextDecorations.Underline) == Twinkle.TextDecorations.Underline)
			{
				fontGlobalStyles |= FontStyles.Underline;
			}

			switch (characterCasing)
			{
				case Twinkle.CharacterCasing.Lower:
					fontGlobalStyles |= FontStyles.LowerCase;
					break;

				case Twinkle.CharacterCasing.Upper:
					fontGlobalStyles |= FontStyles.UpperCase;
					break;
			}

			_textMeshProText.fontStyle = fontGlobalStyles;
		}

		/// <summary>
		/// GameObject TextMeshPro
		/// </summary>

		public Transform TextMeshPro { get; protected set; }

		protected override void AwakeOverride()
		{
			Transform tmp;

			tmp = transform.Find("Tmp");
			TextMeshPro = tmp;

			// On rend invisible le textmeshpro tant qu'il n'a pas de taille calculé
			tmp.localScale = UnityEngine.Vector3.zero;

			_rectTransform = tmp.GetComponent<RectTransform>();
			_textMeshProText = tmp.GetComponent<TextMeshPro>();

			// Description overflowMode
			// mode overflow = bon pour les width infinie
			// mode ellipsis = coupe par lettre ou par mot si enablewordwrapping est à true ne fonctionne pas bien taille (a tendance à mettre des ... à la fin alors qu'il devrait afficher les dernières lettres normalement)
			// mode Linked = semble être comme overflow
			// mode Truncate = pas bon pour les width infinies (n'affiche pas le glyph de fin selon la taille de la chaine) mais ok pour les tailles fixées
			// Description enableWordWrapping
			// Permet de faire du wrapping ou non
			//TextInfo permet de connaitre la taille interne des elements (caractère et ligne)

			// pas de wrapping par défaut
			TextWrapping = Twinkle.TextWrapping.NoWrap;
			TextTrimming = Twinkle.TextTrimming.None;

			// appelle en interne left et top
			SetText(Text);
			SetFontSize(FontSize);
			SetTextAlignment(HorizontalTextAlignment);
			SetVerticalAlignment(VerticalAlignmentOptions.Top);
			//this.SetOutlineThickness(this.outlineThickness);
		}

		public void SetHitTestVisible(bool isHitTestVisible)
		{
			if (isHitTestVisible == true)
			{
				AddCollider();
			}

			if (_textMeshProCollider != null)
			{
				_textMeshProCollider.enabled = isHitTestVisible;
			}
		}

		private BoxCollider AddCollider()
		{
			if (_textMeshProCollider == null)
			{
				var collider = TextMeshPro.gameObject.GetOrAddComponent<BoxCollider>();

				_textMeshProCollider = collider;

				if (collider != null)
				{
					var x = _rectTransform.sizeDelta.x;
					var y = _rectTransform.sizeDelta.y;

					SetColliderSize(x, y);
				}
			}

			return _textMeshProCollider;
		}

		private void SetColliderSize(float width, float height)
		{
			if (_textMeshProCollider != null)
			{
				_textMeshProCollider.size = new UnityEngine.Vector3(width, height, 0.01f);
			}
		}
	}
}