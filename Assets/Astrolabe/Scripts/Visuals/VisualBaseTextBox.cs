using System;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using TMPro;
using UnityEngine;
using FontStyle = Astrolabe.Twinkle.FontStyle;
using FontWeight = Astrolabe.Twinkle.FontWeight;
using TextAlignment = Astrolabe.Twinkle.TextAlignment;

namespace Assets.Astrolabe.Scripts.Visuals
{
	/// <summary>
	/// VisualTextBox qui se fixe sur un TextMeshPro
	/// </summary>
	public class VisualBaseTextBox : VisualElement, IVisualBaseTextBox
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalBaseTextBoxComponent), typeof(VisualBaseTextBox));
		}

		public VisualBaseTextBox(LogicalBaseTextBoxComponent logicalTextBoxBase) : base(logicalTextBoxBase)
		{
		}

		protected LayoutBaseTextBox layoutTextBox;

		public float Opacity
		{
			get => layoutTextBox.Opacity;

			set => layoutTextBox.Opacity = value;
		}

		public string PlaceHolderText
		{
			get => layoutTextBox.PlaceHolderText;

			set => layoutTextBox.PlaceHolderText = value;
		}

		public SolidColorBrush PlaceHolderForeground
		{
			get => layoutTextBox.PlaceHolderForeground;

			set => layoutTextBox.PlaceHolderForeground = value;
		}

		/// <summary>
		/// Text
		/// </summary>

		public virtual string Text
		{
			get => layoutTextBox.Text;

			set => layoutTextBox.Text = value;
		}

		/// <summary>
		/// Doit-on afficher le clavier système sur focus de la textbox ?
		/// </summary>
		public virtual bool ShowSystemKeyboardOnFocus
		{
			get => layoutTextBox.ShowSystemKeyboardOnFocus;
			set
			{
				if (layoutTextBox.ShowSystemKeyboardOnFocus != value)
				{
					layoutTextBox.ShowSystemKeyboardOnFocus = value;
				}
			}
		}

		public event EventHandler<ValueEventArgs<string>> TextChanged;

		public event EventHandler TextSelected;

		public void InvokeTextChanged(string text)
		{
			// Remonte le changement de text au LogicalElement
			TextChanged?.Invoke(this, new ValueEventArgs<string>(text));
		}

		public float FontSize
		{
			get => _fontSize;

			set
			{
				if (_fontSize != value)
				{
					_fontSize = value;
					layoutTextBox.FontSize = value;
				}
			}
		}

		private float _fontSize;

		public float PlaceHolderFontSize
		{
			get => _placeholderFontSize;

			set
			{
				if (_placeholderFontSize != value)
				{
					_placeholderFontSize = value;
					layoutTextBox.PlaceHolderFontSize = value;
				}
			}
		}

		private float _placeholderFontSize;

		public TextAlignment TextAlignment
		{
			get => _textAlignment;

			set
			{
				_textAlignment = value;
				layoutTextBox.TextAlignment = value;
			}
		}

		private TextAlignment _textAlignment = TextAlignment.Left;

		public FontFamily FontFamily
		{
			get => _fontFamily;

			set
			{
				if (_fontFamily != value)
				{
					_fontFamily = value;

					var fontAsset = LoadFontSDF(value);
					layoutTextBox.SetFontSDF(fontAsset);
				}
			}
		}

		private FontFamily _fontFamily;

		public FontFamily PlaceHolderFont
		{
			get => _placeholderFontFamily;

			set
			{
				if (_placeholderFontFamily != value)
				{
					_placeholderFontFamily = value;

					var fontAsset = LoadFontSDF(value);
					layoutTextBox.SetFontSDF(fontAsset, true);
				}
			}
		}

		private FontFamily _placeholderFontFamily;

		protected TMP_FontAsset LoadFontSDF(FontFamily fontFamily)
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
			get => layoutTextBox.Foreground;

			set => layoutTextBox.Foreground = value;
		}

		public CornerRadius CornerRadius
		{
			get => layoutTextBox.CornerRadius;
			set => layoutTextBox.CornerRadius = value;
		}

		// Mesure la taille du TextBlock sans prendre en compte les marges ni dans availableSizeWithoutMargin ni dans la sortie

		public virtual Size MeasureBounds(Size availableSizeWithoutMargin)
		{
			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Width) == false)
			{
				layoutTextBox.Width = availableSizeWithoutMargin.Width;
			}
			else
			{
				// Taille par defaut d'une TextBox
				layoutTextBox.Width = TwinkleApplication.Instance.Framework.Settings.PixelConverter.ToVisual(150);
			}

			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Height) == false)
			{
				layoutTextBox.Height = availableSizeWithoutMargin.Height;
			}
			else
			{
				// Taille par defaut d'une TextBox
				layoutTextBox.Height = TwinkleApplication.Instance.Framework.Settings.PixelConverter.ToVisual(30);
			}

			var bounds = new Size(layoutTextBox.Width, layoutTextBox.Height);

			return bounds;
		}

		public void SetFontStyle(FontStyle fontStyle, FontWeight fontWeight, TextDecorations textDecoration,
			CharacterCasing characterCasing, bool placeholder = false)
		{
			layoutTextBox.SetFontStyle(fontStyle, fontWeight, textDecoration, characterCasing, placeholder);
		}

		public override void SetHitTestVisible(bool value)
		{
			layoutTextBox.SetHitTestVisible(value);
		}

		public void InvokeTextSelected()
		{
			TextSelected?.Invoke(this, EventArgs.Empty);
		}

		public void SelectText()
		{
			layoutTextBox.SelectText();
		}

		public void DeselectText()
		{
			layoutTextBox.DeselectText();
		}

		/// <summary>
		/// MultiLine Enabled
		/// </summary>

		public virtual bool IsMultiLineEnabled
		{
			get => isMultiLineEnabled;

			set
			{
				if (IsMultiLineEnabled != value)
				{
					isMultiLineEnabled = value;
					layoutTextBox.IsMultiLineEnabled = value;
				}
			}
		}

		protected bool isMultiLineEnabled = false;
	}
}