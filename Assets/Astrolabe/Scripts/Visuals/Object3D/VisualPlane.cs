using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public class VisualPlane : VisualObject3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalPlane), typeof(VisualPlane));
		}

		public VisualPlane(ILogicalElement logicalElement) : base(logicalElement)
		{
			Initialize(TwinklePrefabFactory.Instance.prefabPlane);
		}
	}
}