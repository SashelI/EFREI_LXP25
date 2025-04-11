using System;
using MixedReality.Toolkit;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	[AddComponentMenu("MRTK/UX/Stateful Interactable Switch Toggle Visuals")]
	public class AstrolabeSwitchToggleVisuals : MonoBehaviour
	{
		#region Serialized Fields
		[SerializeField]
		public StatefulInteractable statefulInteractable;

		[SerializeField]
		[Tooltip("The GameObject that contains the toggle thumb.")]
		public GameObject toggleRoot;

		[SerializeField]
		[Tooltip("The relative space from the switch's origin that is covered. Typically set to 0.5f")]
		public float toggleOffset = 0.5f;

		[Header("Easing")]
		[SerializeField]
		public float duration = 0.2f;

		[SerializeField]
		public AnimationCurve animationCurve;

		[SerializeField] public MeshRenderer backplateMesh;

		private const string FADE_OUT_PROPERTY = "_Fade_Out_";

		#endregion

		#region MonoBehaviours

		/// <summary>
		/// A Unity event function that is called when an enabled script instance is being loaded.
		/// </summary>
		protected virtual void Start()
		{
			// If the StatefulInteractable is null, 
			if (statefulInteractable == null)
			{
				statefulInteractable = GetComponent<StatefulInteractable>();
			}

			if (statefulInteractable != null)
			{
				// Initializing the toggle state
				bool isToggled = statefulInteractable.IsToggled;

				if (isToggled)
				{
					toggleRoot.transform.localPosition = Vector3.right * toggleOffset;
					SetBackplateEffectState(1.0f);
				}
				else
				{
					toggleRoot.transform.localPosition = Vector3.left * toggleOffset;
					SetBackplateEffectState(0.1f);
				}

				lastToggleState = isToggled;
			}
		}

		/// <summary>
		/// A Unity event function that is called every frame after normal update functions, if this object is enabled.
		/// </summary>
		protected virtual void LateUpdate()
		{
			UpdateAllVisuals();
		}

		protected virtual void OnDestroy()
		{
			backplateMesh = null;
		}
		#endregion

		#region Visuals

		// Used to ensure we only update visuals when the toggle state changes
		private bool lastToggleState;

		// Used to animate the switch toggle based on the assignable easing properties
		private float animationTimer = float.MaxValue;

		private void UpdateAllVisuals()
		{
			bool isToggled = statefulInteractable.IsToggled;

			if (lastToggleState != isToggled)
			{
				animationTimer = 0.0f;
				lastToggleState = isToggled;
			}

			if (animationTimer < duration)
			{
				animationTimer += Time.deltaTime;
				if (isToggled)
				{
					toggleRoot.transform.localPosition = Vector3.Lerp(Vector3.left * toggleOffset, Vector3.right * toggleOffset, animationCurve.Evaluate(animationTimer / duration));
					SetBackplateEffectState(Mathf.Lerp(0.1f, 1.0f, animationCurve.Evaluate(animationTimer / duration)));
				}
				else
				{
					toggleRoot.transform.localPosition = Vector3.Lerp(Vector3.right * toggleOffset, Vector3.left * toggleOffset, animationCurve.Evaluate(animationTimer / duration));
					SetBackplateEffectState(Mathf.Lerp(1.0f, 0.1f, animationCurve.Evaluate(animationTimer / duration)));
				}
			}
		}

		private void SetBackplateEffectState(float value)
		{
			if (backplateMesh != null)
			{
				backplateMesh.material?.SetFloat(FADE_OUT_PROPERTY, value);
			}
		}


		private void OnDrawGizmos()
		{
			Vector3 toggleRelativeScale = toggleRoot.transform.lossyScale;
			float toggleGizmoScaleFactor = 0.001f;

			Gizmos.color = Color.gray;
			Gizmos.DrawSphere(toggleRoot.transform.parent.position + toggleOffset * toggleRelativeScale.x * Vector3.left, toggleRelativeScale.magnitude * toggleGizmoScaleFactor);

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(toggleRoot.transform.parent.position + toggleOffset * toggleRelativeScale.x * Vector3.right, toggleRelativeScale.magnitude * toggleGizmoScaleFactor);
		}

		#endregion
	}
}
