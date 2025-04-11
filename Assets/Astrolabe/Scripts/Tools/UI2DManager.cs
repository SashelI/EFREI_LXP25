using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Astrolabe.Scripts.Tools
{
	public class UI2DManager : MonoBehaviour
	{
		private void Start()
		{
			var twinkle = gameObject.GetComponent<TwinkleComponent>();

			//Les éléments 2D ont besoin de certains composants pour supporter les interactions
			if (twinkle.is2DElement) //ATTENTION ceci est la valeur au start du component
			{
				if (twinkle.VisualElement.LogicalElement is LogicalButton logicalButton)
				{
					var button = gameObject.AddComponent<Button>();
					button.onClick.AddListener(()=>
					{
						SendUIButtonClick(logicalButton);
					});

					var image = gameObject.AddComponent<Image>();
					image.color = UnityEngine.Color.clear;

					gameObject.GetComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(twinkle.Width, twinkle.Height);
				}
			}
		}

		private void SendUIButtonClick(LogicalButton button)
		{
			button.RaiseOnClick();
		}
	}
}
