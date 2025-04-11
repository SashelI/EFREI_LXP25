using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public class VisualCapsule : VisualObject3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalCapsule), typeof(VisualCapsule));
		}

		public VisualCapsule(ILogicalElement logicalElement) : base(logicalElement)
		{
			Initialize(TwinklePrefabFactory.Instance.prefabCapsule);
		}
	}
}