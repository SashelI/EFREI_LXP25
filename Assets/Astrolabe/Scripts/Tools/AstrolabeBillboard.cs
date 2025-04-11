using UnityEngine;
using MixedReality.Toolkit;
using UnityEngine.Serialization;

namespace Assets.Astrolabe.Scripts.Tools
{
	public enum PivotAxis
	{
		// Most common options, preserving current functionality with the same enum order.
		XY,
		Y,

		// Rotate about an individual axis.
		X,
		Z,

		// Rotate about a pair of axes.
		XZ,
		YZ,

		// Rotate about all axes.
		Free
	}

	/// <summary>
	/// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
	/// </summary>
	[AddComponentMenu("Scripts/MRTK/SDK/Billboard")]
	public class AstrolabeBillboard : MonoBehaviour
	{
		[FormerlySerializedAs("IsParentRotationLocked")] public bool isParentRotationLocked = false;

		/// <summary>
		/// The axis about which the object will rotate.
		/// </summary>
		public PivotAxis PivotAxis
		{
			get => pivotAxis;
			set => pivotAxis = value;
		}

		[Tooltip("Specifies the axis about which the object will rotate.")] [SerializeField]
		private PivotAxis pivotAxis = PivotAxis.XY;

		/// <summary>
		/// The target we will orient to. If no target is specified, the main camera will be used.
		/// </summary>
		public Transform TargetTransform
		{
			get => targetTransform;
			set => targetTransform = value;
		}

		[Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
		[SerializeField]
		private Transform targetTransform;

		private void OnEnable()
		{
			if (targetTransform == null)
			{
				targetTransform = Camera.main.transform;
			}
		}

		/// <summary>
		/// Keeps the object facing the camera.
		/// </summary>
		private void Update()
		{
			if (targetTransform == null)
			{
				return;
			}

			// Get a Vector that points from the target to the main camera.
			var directionToTarget = targetTransform.position - transform.position;
			Vector3 localAxe;

			var useCameraAsUpVector = true;

			// Adjust for the pivot axis.
			switch (pivotAxis)
			{
				case PivotAxis.X:
					directionToTarget.x = 0.0f;
					localAxe = Vector3.right;
					useCameraAsUpVector = false;
					break;

				case PivotAxis.Y:
					directionToTarget.y = 0.0f;
					localAxe = Vector3.up;
					useCameraAsUpVector = false;
					break;

				case PivotAxis.Z:
					directionToTarget.x = 0.0f;
					directionToTarget.y = 0.0f;
					localAxe = Vector3.forward;
					break;

				case PivotAxis.XY:
					useCameraAsUpVector = false;
					localAxe = new Vector3(1f, 1f, 0);
					break;

				case PivotAxis.XZ:
					directionToTarget.x = 0.0f;
					localAxe = new Vector3(1f, 0f, 1f);
					break;

				case PivotAxis.YZ:
					directionToTarget.y = 0.0f;
					localAxe = new Vector3(0f, 1f, 1f);
					break;

				case PivotAxis.Free:
				default:
					localAxe = Vector3.one;
					// No changes needed.
					break;
			}

			// If we are right next to the camera the rotation is undefined. 
			if (directionToTarget.sqrMagnitude < 0.001f)
			{
				return;
			}

			// Calculate and apply the rotation required to reorient the object
			if (useCameraAsUpVector)
			{
				transform.rotation = Quaternion.LookRotation(-directionToTarget, Camera.main.transform.up);
			}
			else
			{
				//transform.rotation = Quaternion.LookRotation(-directionToTarget);
				transform.rotation = Quaternion.LookRotation(-directionToTarget);
			}

			if (isParentRotationLocked)
			{
				// Cela permet de faire une rotation local en prenant en compte la rotation du parent (sauf sur l'axe du biboard qui vise la camera)
				var angle = transform.localEulerAngles;
				var v = new Vector3(angle.x * localAxe.x, angle.y * localAxe.y, angle.z * localAxe.z);
				transform.localEulerAngles = v;
			}
		}
	}
}