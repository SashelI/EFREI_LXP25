using System;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using FontStyle = Astrolabe.Twinkle.FontStyle;
using FontWeight = Astrolabe.Twinkle.FontWeight;
using Object = UnityEngine.Object;
using TextAlignment = Astrolabe.Twinkle.TextAlignment;

namespace Assets.Astrolabe.Scripts.Visuals
{
	/// <summary>
	/// Rendu de TextBlock
	/// </summary>
	public class VisualTextBlock : VisualElement, IVisualTextBlock
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalTextBlock), typeof(VisualTextBlock));
		}

		public VisualTextBlock(ILogicalElement logicalElement) : base(logicalElement)
		{
			var transform = Object.Instantiate(TwinklePrefabFactory.Instance.prefabTextMeshPro);

			textMeshPro = transform.GetComponent<LayoutTextMeshPro>();
			sortingGroup = transform.GetComponentInChildren<SortingGroup>();

			TwinkleComponent = textMeshPro;
		}

		protected LayoutTextMeshPro textMeshPro;
		protected SortingGroup sortingGroup;

		public float Opacity
		{
			get => textMeshPro.Opacity;

			set => textMeshPro.Opacity = value;
		}

		/// <summary>
		/// Text
		/// </summary>

		public string Text
		{
			get => textMeshPro.Text;

			set => textMeshPro.Text = value;
		}

		public float FontSize
		{
			get => _fontSize;

			set
			{
				if (_fontSize != value)
				{
					_fontSize = value;
					textMeshPro.FontSize = value;
				}
			}
		}

		private float _fontSize;

		public TextAlignment HorizontalTextAlignment
		{
			get => _horizontalTextAlignment;

			set
			{
				_horizontalTextAlignment = value;
				textMeshPro.HorizontalTextAlignment = value;
			}
		}
		private TextAlignment _horizontalTextAlignment = TextAlignment.Left;

		public VerticalAlignment VerticalTextAlignment
		{
			get => _verticalTextAlignment;

			set
			{
				_verticalTextAlignment = value;
				textMeshPro.VerticalTextAlignment = value;
			}
		}
		private VerticalAlignment _verticalTextAlignment = VerticalAlignment.Top;

		public TextWrapping TextWrapping
		{
			get => _textWrapping;

			set
			{
				_textWrapping = value;
				textMeshPro.TextWrapping = value;
			}
		}

		private TextWrapping _textWrapping = TextWrapping.NoWrap;

		public TextTrimming TextTrimming
		{
			get => _textTrimming;

			set
			{
				_textTrimming = value;
				textMeshPro.TextTrimming = value;
			}
		}

		private TextTrimming _textTrimming = TextTrimming.None;

		public FontFamily FontFamily
		{
			get => _fontFamily;

			set
			{
				if (_fontFamily != value)
				{
					_fontFamily = value;

					var fontAsset = LoadFontSDF(value);
					textMeshPro.SetFontSDF(fontAsset);
				}
			}
		}

		private FontFamily _fontFamily;

		private TMP_FontAsset LoadFontSDF(FontFamily fontFamily)
		{
			if (fontFamily != null && fontFamily.Source != null)
			{
				if (fontFamily.IsLoaded == true)
				{
					return fontFamily.Content as TMP_FontAsset;
				}

				try
				{
					var fullFilename = UriInformation.ParseToRead(fontFamily.Source).GetPath();

					if (TwinkleApplication.Instance.IsDesignMode)
					{
						// en mode Design on ne peut pas charger directement le fullname qui n'appartient pas à l'app même en utilisant BroadAccessFileSystem dans le Manager
						// On va donc charger la font, ecrire un fichier dans data:/// et utiliser ce fichier temporaire. Ensuite on pourra lire le fichier normalemebt via Font(filename)

						var result = FileHelper.ReadAllBytes(fullFilename);

						if (result.Error != null)
						{
							Log.WriteLine("LoadFontSDF Exception=" + result.Error.Message);

							throw result.Error;
						}

						FileHelper.WriteBytes(result.Content, "__Font.temp", StorageType.Persistent);
						fullFilename = new Uri("data:///__Font.temp").GetFullFilename();
					}

					var font = new Font(fullFilename);
					var fontAsset = TMP_FontAsset.CreateFontAsset(font);
					fontFamily.Content = fontAsset;

					return fontAsset;
				}
				catch (Exception ex)
				{
					Log.WriteLine(ex);
				}
			}

			return null;
		}

		public SolidColorBrush Foreground
		{
			get => textMeshPro.Foreground;

			set => textMeshPro.Foreground = value;
		}

		public int SortingOrder
		{
			get => sortingGroup.sortingOrder;
			set => sortingGroup.sortingOrder = value;
		}

		public bool IsRichTextBlockEnabled
		{
			get => textMeshPro.IsRichTextBlockEnabled;
			set => textMeshPro.IsRichTextBlockEnabled = value;
		}

		/*
		public float OutlineThickness
		{
		    get
		    {
		        return this.textMeshPro.OutlineThickness;
		    }

		    set
		    {
		        this.textMeshPro.OutlineThickness = value;
		    }
		}

		public SolidColorBrush OutlineColor
		{
		    get
		    {
		        return this.textMeshPro.OutlineColor;
		    }

		    set
		    {
		        this.textMeshPro.OutlineColor = value;
		    }
		}
		*/

		// Mesure la taille du TextBlock sans prendre en compte les marges ni dans availableSizeWithoutMargin ni dans la sortie

		public Size MeasureBounds(Size availableSizeWithoutMargin)
		{
			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Width) == false)
			{
				textMeshPro.Width = availableSizeWithoutMargin.Width;
			}
			else
			{
				// la taille sera refixée plus tard avec le resultat du GetRenderValues et seront appliqué dans le resize
				textMeshPro.Width = 99999;
			}

			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Height) == false)
			{
				textMeshPro.Height = availableSizeWithoutMargin.Height;
			}
			else
			{
				textMeshPro.Height = 99999;
			}

			var bounds = textMeshPro.GetBounds(availableSizeWithoutMargin);

			return bounds;
		}

		public void SetFontStyle(FontStyle fontStyle, FontWeight fontWeight, TextDecorations textDecoration,
			CharacterCasing characterCasing)
		{
			textMeshPro.SetFontStyle(fontStyle, fontWeight, textDecoration, characterCasing);
		}

		public override void SetHitTestVisible(bool value)
		{
			textMeshPro.SetHitTestVisible(value);
		}
	}
}