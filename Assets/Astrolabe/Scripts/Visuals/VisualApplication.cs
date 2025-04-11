using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Object = UnityEngine.Object;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualApplication : VisualElement, IVisualApplication
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalApplication), typeof(VisualApplication));
		}

		public VisualApplication(ILogicalElement logicalElement) : base(logicalElement)
		{
			_application = Object.Instantiate(TwinklePrefabFactory.Instance.prefabApplication)
				.GetComponent<LayoutApplication>();
			TwinkleComponent = _application;
		}

		private readonly LayoutApplication _application;
	}
}