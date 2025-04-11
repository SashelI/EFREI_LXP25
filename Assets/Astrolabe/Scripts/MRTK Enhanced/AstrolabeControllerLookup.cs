using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
    /// <summary>
    /// A basic convenience registry allowing easy reference
    /// to controllers.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MRTK/Core/Controller Lookup")]
    public class AstrolabeControllerLookup : ControllerLookup
    {
        // Magicleap Controller
        [SerializeField]
        [Tooltip("The camera rig's right hand controller.")]
        private XRBaseController mlController = null;

        /// <summary>
        /// The camera rig's right hand controller.
        /// </summary>
        public XRBaseController MLController
        {
	        get => mlController;
	        set => mlController = value;
        }

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (FindObjectUtility.FindObjectsByType<AstrolabeControllerLookup>(false, false).Length > 1)
            {
                Debug.LogWarning("Found more than one instance of the AstrolabeControllerLookup class in the hierarchy. There should only be one");
            }
        }
    }
}
