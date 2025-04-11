using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public class VisualCube : VisualObject3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalCube), typeof(VisualCube));
		}

		public VisualCube(ILogicalElement logicalElement) : base(logicalElement)
		{
			Initialize(TwinklePrefabFactory.Instance.prefabCube);
		}
	}
}