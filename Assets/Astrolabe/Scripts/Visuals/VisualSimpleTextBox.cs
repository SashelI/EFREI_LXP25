using System;
using System.Collections.Generic;
using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Object = UnityEngine.Object;

namespace Assets.Astrolabe.Scripts.Visuals
{
	/// <summary>
	/// VisualTextBox qui se fixe sur un TextMeshPro
	/// </summary>
	public class VisualSimpleTextBox : VisualBaseTextBox, IVisualSimpleTextBoxComponent
	{
		public new static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalSimpleTextBoxComponent), typeof(VisualSimpleTextBox));
		}

		public VisualSimpleTextBox(LogicalSimpleTextBoxComponent logicalSimpleTextBox) : base(logicalSimpleTextBox)
		{
			layoutTextBox = Object.Instantiate(TwinklePrefabFactory.Instance.prefabSimpleTextBox)
				.GetComponent<LayoutSimpleTextBox>();
			LayoutSimpleTextBox.VisualElement = this;

			TwinkleComponent = LayoutSimpleTextBox;
		}

		private LayoutSimpleTextBox LayoutSimpleTextBox => layoutTextBox as LayoutSimpleTextBox;

		/// <summary>
		/// Curseur visible ?
		/// </summary>
		public bool IsCaretVisible
		{
			get => LayoutSimpleTextBox.IsCaretVisible;
			set
			{
				if (LayoutSimpleTextBox.IsCaretVisible != value)
				{
					LayoutSimpleTextBox.IsCaretVisible = value;
				}
			}
		}

		/// <summary>
		/// Position du curseur
		/// </summary>
		public int CaretPosition
		{
			get => LayoutSimpleTextBox.CaretPosition;
			set => LayoutSimpleTextBox.CaretPosition = value;
		}

		/// <summary>
		/// Index de la ligne du caret
		/// </summary>
		public int CaretLineIndex => LayoutSimpleTextBox.CaretLineIndex;

		/// <summary>
		/// Taille du curseur
		/// </summary>
		public int CaretWidth
		{
			get => LayoutSimpleTextBox.CaretWidth;
			set
			{
				if (LayoutSimpleTextBox.CaretWidth != value)
				{
					LayoutSimpleTextBox.CaretWidth = value;
				}
			}
		}

		/// <summary>
		/// Doit-on afficher le clavier système sur focus de la textbox ?
		/// </summary>
		public override bool ShowSystemKeyboardOnFocus
		{
			get => LayoutSimpleTextBox.ShowSystemKeyboardOnFocus;
			set
			{
				if (LayoutSimpleTextBox.ShowSystemKeyboardOnFocus != value)
				{
					LayoutSimpleTextBox.ShowSystemKeyboardOnFocus = value;
				}
			}
		}

		/// <summary>
		/// Couleur de la sélection du texte
		/// </summary>
		public SolidColorBrush SelectionColor
		{
			get => LayoutSimpleTextBox.SelectionColor;
			set
			{
				if (LayoutSimpleTextBox.SelectionColor != value)
				{
					LayoutSimpleTextBox.SelectionColor = value;
				}
			}
		}

		/// <summary>
		/// Gestion manuelle de la sélection du texte ?
		/// (permet de gérer via des actions externes la sélection du texte de l'input)
		/// (et ainsi de ne pas déclencher le onSelect de l'inputfield mrtk côté Foundation)
		/// </summary>
		public bool ManualTextSelectionManagement
		{
			get => LayoutSimpleTextBox.ManualTextSelectionManagement;
			set
			{
				if (LayoutSimpleTextBox.ManualTextSelectionManagement != value)
				{
					LayoutSimpleTextBox.ManualTextSelectionManagement = value;
				}
			}
		}

		/// <summary>
		/// Text
		/// </summary>

		public override string Text
		{
			get => LayoutSimpleTextBox.Text;

			set => LayoutSimpleTextBox.Text = value;
		}

		public event EventHandler<ValueEventArgs<string>> TextChanged;

		public event EventHandler TextSelected;
		public event EventHandler<ValueEventArgs<List<string>>> VisualLinesParsedChanged;

		/// <summary>
		/// On avertit le Core du changement des lignes "visuelles"
		/// </summary>
		/// <param name="visualLines"></param>
		public void InvokeVisualLinesChanged(List<string> visualLines)
		{
			VisualLinesParsedChanged?.Invoke(this, new ValueEventArgs<List<string>>(visualLines));
		}

		// Mesure la taille du TextBlock sans prendre en compte les marges ni dans availableSizeWithoutMargin ni dans la sortie
		public override Size MeasureBounds(Size availableSizeWithoutMargin)
		{
			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Width) == false)
			{
				LayoutSimpleTextBox.Width = availableSizeWithoutMargin.Width;
			}
			else
			{
				// Taille par defaut d'une TextBox
				LayoutSimpleTextBox.Width = TwinkleApplication.Instance.Framework.Settings.PixelConverter.ToVisual(150);
			}

			if (float.IsPositiveInfinity(availableSizeWithoutMargin.Height) == false)
			{
				LayoutSimpleTextBox.Height = availableSizeWithoutMargin.Height;
			}
			else
			{
				// Taille par defaut d'une TextBox
				LayoutSimpleTextBox.Height = TwinkleApplication.Instance.Framework.Settings.PixelConverter.ToVisual(30);
			}

			var bounds = new Size(LayoutSimpleTextBox.Width, LayoutSimpleTextBox.Height);

			return bounds;
		}

		/// <summary>
		/// MultiLine Enabled
		/// </summary>

		public override bool IsMultiLineEnabled
		{
			get => isMultiLineEnabled;

			set
			{
				if (IsMultiLineEnabled != value)
				{
					isMultiLineEnabled = value;
					LayoutSimpleTextBox.IsMultiLineEnabled = value;
				}
			}
		}

		public void NewBoxFocused()
		{
			LayoutSimpleTextBox.firstTextSet = false;
		}
	}
}