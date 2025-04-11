using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public class VisualQuad : VisualObject3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalQuad), typeof(VisualQuad));
		}

		public VisualQuad(ILogicalElement logicalElement) : base(logicalElement)
		{
			Initialize(TwinklePrefabFactory.Instance.prefabQuad);
		}
	}
}