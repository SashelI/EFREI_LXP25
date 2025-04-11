using System.Collections;
using System.Threading.Tasks;
using Astrolabe.Diagnostics;
using UnityEngine;

namespace Assets.Astrolabe.Scripts
{
	public class AnchorManager : MonoBehaviour
	{
		private bool _anchorSaved = false;
		private OVRSpatialAnchor _anchor;

		private void Start()
		{
#if !UNITY_EDITOR
		    if (!_anchorSaved)
		    {
			    StartCoroutine(CreateSpatialAnchor());
		    }
#endif
		}

		IEnumerator CreateSpatialAnchor()
		{
			_anchor = gameObject.AddComponent<OVRSpatialAnchor>();

			// Wait for the async creation
			yield return new WaitUntil(() => _anchor.Created);

			Log.WriteLine($"Spatial Anchor created with id : {_anchor.Uuid}.", LogMessageType.Information);

			Task.Run(async () =>
			{
				await SaveSpatialAnchor(_anchor);
			});
		}

		public async Task SaveSpatialAnchor(OVRSpatialAnchor anchor)
		{
			var result = await anchor.SaveAnchorAsync();
			if (result.Success)
			{
				_anchorSaved = true;
				Log.WriteLine("Spatial Anchor saved locally.", LogMessageType.Information);
			}
			else
			{
				Log.WriteLine($"Failed Saving Spatial Anchor : {result.Status == OVRAnchor.SaveResult.FailureInvalidAnchor} , {result.Status == OVRAnchor.SaveResult.Success}", LogMessageType.Error);
			}
		}
	}
}