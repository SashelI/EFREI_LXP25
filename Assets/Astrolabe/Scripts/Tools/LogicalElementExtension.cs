using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class LogicalElementExtension
	{
		public static GameObject GetGameObject(this ILogicalElement logicalElement)
		{
			return ((TwinkleComponent)logicalElement.VisualElement.Element).gameObject;
		}

		public static Transform GetRenderTransform(this ILogicalElement logicalElement)
		{
			var component = (TwinkleComponent)logicalElement.VisualElement.Element;

			return component.GetRenderTransform();
		}
	}
}