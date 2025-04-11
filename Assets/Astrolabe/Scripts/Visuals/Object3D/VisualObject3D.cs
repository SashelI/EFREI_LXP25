using Assets.Astrolabe.Scripts.Components.Object3D;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Visuals.Object3D
{
	public abstract class VisualObject3D : VisualFluentElement, IVisualObject3D
	{
		public VisualObject3D(ILogicalElement logicalElement) : base(logicalElement)
		{
		}

		protected void Initialize(Transform prefab)
		{
			_object3D = Object.Instantiate(prefab).GetComponent<LayoutObject3D>();

			Debug.Assert(_object3D != null, "No LayoutObject3D found in this prefab!");

			FluentShader = _object3D.FluentShader;
			TwinkleComponent = _object3D;
		}

		private LayoutObject3D _object3D;

		public bool IsDepthCentered
		{
			get => _object3D.IsDepthCentered;
			set => _object3D.IsDepthCentered = value;
		}
	}
}