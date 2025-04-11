using Assets.Astrolabe.Scripts.Tools;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutModel3D : TwinkleComponent
	{
		private Transform _object3D;
		private Transform _root3D;
		private Transform _scaleObject3D;

		public bool IsModelLoaded { get; set; } = false;

		protected override void SetWidthOverride(float width)
		{
			if (IsModelLoaded == true)
			{
				var scale = _scaleObject3D.localScale;
				_scaleObject3D.localScale = new Vector3(width, scale.y, scale.z);
			}
			else
			{
				_scaleObject3D.localScale = new Vector3(1, 1, 1);
			}
		}

		protected override void SetHeightOverride(float height)
		{
			if (IsModelLoaded == true)
			{
				var scale = _scaleObject3D.localScale;
				_scaleObject3D.localScale = new Vector3(scale.x, height, scale.z);
			}
			else
			{
				_scaleObject3D.localScale = new Vector3(1, 1, 1);
			}
		}

		protected override void SetDepthOverride(float depth)
		{
			if (IsModelLoaded == true)
			{
				var scale = _scaleObject3D.localScale;
				_scaleObject3D.localScale = new Vector3(scale.x, scale.y, depth);
			}
			else
			{
				_scaleObject3D.localScale = new Vector3(1, 1, 1);
			}
		}

		public void SetCenter(global::Astrolabe.Twinkle.Vector3 center)
		{
			_object3D.localPosition = -center.ToVector3();
		}

		public void SetObject3D(GameObject objectLoaded)
		{
			if (_root3D != null)
			{
				_root3D.SetParent(null, false);
				Destroy(_root3D.gameObject);
			}

			if (objectLoaded != null)
			{
				objectLoaded.transform.SetParent(_object3D, false);
				_root3D = objectLoaded.transform;
			}
		}

		// Start is called before the first frame update
		protected override void AwakeOverride()
		{
			_scaleObject3D = transform.Find("ScaleObject3D");
			_object3D = _scaleObject3D.Find("Object3D");

			SetDepth(Depth);
		}

		private void Update()
		{
			//if (this.scaleObject3D != null)
			//{
			//	scaleObject3D.position = UnityEngine.Vector3.zero;
			//}

			//if (this.object3D != null)
			//{
			//	object3D.position = UnityEngine.Vector3.zero;
			//}

			transform.localPosition = Vector3.zero;
		}
	}
}