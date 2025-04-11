using UnityEngine;

namespace Assets.Scripts.Tools.Helpers
{
    public static class LayerHelper
    {
        /// <summary>
        /// Recursive helper function to set every childs index layer 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerGameObjectAndChilds(GameObject go , int layer)
        {
            if (go != null) go.layer = layer;
            foreach(Transform tr in go.transform)
            {
                SetLayerGameObjectAndChilds(tr.gameObject, layer);
            }
        }


        /// <summary>
        /// Recursive helper function to set every childs layer name
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerGameObjectAndChilds(GameObject go, string layer)
        {
            if (go != null) go.layer = LayerMask.NameToLayer(layer);
            foreach (Transform tr in go.transform)
            {
                SetLayerGameObjectAndChilds(tr.gameObject, layer);
            }
        }
    }
}
