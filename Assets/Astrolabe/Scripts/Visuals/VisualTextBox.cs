using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Visuals
{
	/// <summary>
	/// VisualTextBox qui se fixe sur un TextMeshPro
	/// </summary>
	public class VisualTextBox : VisualBaseTextBox, IVisualTextBoxComponent
	{
		public new static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalTextBoxComponent), typeof(VisualTextBox));
		}

		public VisualTextBox(LogicalTextBoxComponent logicalTextBox) : base(logicalTextBox)
		{
			layoutTextBox = Object.Instantiate(TwinklePrefabFactory.Instance.prefabTextBox).GetComponent<LayoutBaseTextBox>();
			layoutTextBox.VisualElement = this;

			TwinkleComponent = layoutTextBox;
		}
	}
}