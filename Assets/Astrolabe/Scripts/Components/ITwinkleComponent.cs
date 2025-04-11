using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Components
{
	public interface ITwinkleComponent
	{
		float Left { get; set; }

		float Top { get; set; }

		float Width { get; set; }

		float Height { get; set; }

		float Forward { get; set; }

		VisualElement VisualElement { get; set; }

		void AddChild(IVisualElement child);
		void RemoveChild(IVisualElement chid);

		Vector3 Rotation { get; set; }

		Vector3 Scale { get; set; }

		Vector3 Translation { get; set; }
	}
}