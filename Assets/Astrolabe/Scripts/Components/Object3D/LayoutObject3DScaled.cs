using Assets.Astrolabe.Scripts.Tools;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Components.Object3D
{
	public class LayoutObject3DScaled : LayoutObject3D
	{
		protected override Transform GetObject3DTransform()
		{
			var object3DTransform = transform.FindRecursively("Object3DTransform");

			return object3DTransform;
		}
	}
}