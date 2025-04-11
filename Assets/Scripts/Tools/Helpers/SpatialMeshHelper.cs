using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Assets.Scripts.Tools.Helpers
{
	public static class SpatialMeshHelper
	{
		private static ARMeshManager _meshManager;

		public static void EnableMeshManager(bool enable)
		{
			if (_meshManager == null)
			{
				var meshManagerGo = GameObject.FindGameObjectWithTag("MeshManager");

				if (meshManagerGo != null)
				{
					_meshManager = meshManagerGo.GetComponent<ARMeshManager>();
				}
			}

			if (_meshManager != null)
			{
				if (_meshManager.enabled != enable)
				{
					_meshManager.enabled = enable;
				}
			}
		}
	}
}