using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public class VisualSphere : VisualObject3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalSphere), typeof(VisualSphere));
		}

		public VisualSphere(ILogicalElement logicalElement) : base(logicalElement)
		{
			Initialize(TwinklePrefabFactory.Instance.prefabSphere);
		}
	}
}