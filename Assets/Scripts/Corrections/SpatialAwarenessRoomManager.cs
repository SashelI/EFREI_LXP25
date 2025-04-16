using Meta.XR.MRUtilityKit;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Harbor.Platforms.Meta_Quest
{
	/// <summary>
	/// Sets the right Quest's spatial mesh layer and parent for occlusion tool
	/// </summary>
    public class SpatialAwarenessRoomManager : MonoBehaviour
    {
	    public void SetRoomParentAndLayer()
	    {
		    foreach (var room in MRUK.Instance.Rooms)
		    {
			    SetRoomParentAndLayer(room);
		    }
		}
	    public void SetRoomParentAndLayer(MRUKRoom room)
	    {
			room.transform.SetParent(transform, true);
			room.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Spatial Awareness"));
	    }
	}
}
