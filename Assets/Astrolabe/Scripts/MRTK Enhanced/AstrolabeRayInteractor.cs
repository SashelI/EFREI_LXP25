using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityInputSystem = UnityEngine.InputSystem;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	/// <summary>
	/// MRTK Ray Interactor override to handle Quest hands, quest controllers, HL hands correctly with OpenXR
	/// </summary>
	public class AstrolabeRayInteractor : MRTKRayInteractor
	{
		#region MRTKRayInteractor

		private float _refDistance = 0;

		private Pose _initialLocalAttach = Pose.identity;

		private XRInteractionManager _interactionManager;

		#endregion MRTKRayInteractor

		#region XRBaseInteractor

		[SerializeField]
		[Tooltip("The input action we key into to determine whether this controller is tracked or not")]
		private InputActionProperty controllerDetectedAction;

		/// <summary>
		/// True if the input is from a controller
		/// </summary>
		private bool _isController;

		/// <inheritdoc />
		protected override void OnSelectEntering(SelectEnterEventArgs args)
		{
			base.OnSelectEntering(args);

			_initialLocalAttach = new Pose(attachTransform.localPosition, attachTransform.localRotation);
			_refDistance = AstrolabePoseUtilities.GetDistanceToBody(new Pose(transform.position, transform.rotation));
		}

		#endregion XRBaseInteractor

		public UnityEvent<UnityInputSystem.XR.XRController, InteractorHandedness> ControllerDetected;
		public UnityEvent<TrackedDevice, InteractorHandedness> TrackedDeviceDetected;
		public UnityEvent<UnityInputSystem.XR.XRController, InteractorHandedness> ControllerLost;
		public UnityEvent<TrackedDevice, InteractorHandedness> TrackedDeviceLost;

		protected override void Start()
		{
			base.Start();

			if (controllerDetectedAction == null || controllerDetectedAction.action == null)
			{
				return;
			}

			controllerDetectedAction.action.started += ControllerStarted;
			controllerDetectedAction.action.canceled += ControllerEnded;
			controllerDetectedAction.EnableDirectAction();
		}

		private void ControllerStarted(InputAction.CallbackContext context)
		{
			if (context.control.device is UnityInputSystem.XR.XRController controller)
			{
				_isController = true;
				ControllerDetected?.Invoke(controller, handedness);
			}
			else if (context.control.device is TrackedDevice trackedDevice)
			{
				TrackedDeviceDetected?.Invoke(trackedDevice, handedness);
			}
		}

		private void ControllerEnded(InputAction.CallbackContext context)
		{
			if (context.control.device is UnityInputSystem.XR.XRController controller)
			{
				_isController = false;
				ControllerLost?.Invoke(controller, handedness);
			}
			else if (context.control.device is TrackedDevice trackedDevice)
			{
				TrackedDeviceLost?.Invoke(trackedDevice, handedness);
			}
		}

		/// <summary>
		/// A Unity event function that is called every frame, if this object is enabled.
		/// </summary>
		private void Update()
		{
			if (AimPoseSource is AstrolabeFallbackPoseSource astrolabePoseSource)
			{
				astrolabePoseSource.IsController = _isController;

				if (astrolabePoseSource.TryGetPose(out var aimPose))
				{
					transform.SetPositionAndRotation(aimPose.position, aimPose.rotation);

					if (hasSelection)
					{
						var distanceRatio = AstrolabePoseUtilities.GetDistanceToBody(aimPose) / _refDistance;
						attachTransform.localPosition = 
							new Vector3(_initialLocalAttach.position.x, _initialLocalAttach.position.y, _initialLocalAttach.position.z * distanceRatio);
					}
				}
			}
			else
			{
				// Use Pose Sources to calculate the interactor's pose and the attach transform's position
				// We have to make sure the ray interactor is oriented appropriately before calling
				// lower level raycasts
				if (AimPoseSource != null && AimPoseSource.TryGetPose(out var aimPose))
				{
					transform.SetPositionAndRotation(aimPose.position, aimPose.rotation);

					if (hasSelection)
					{
						var distanceRatio = AstrolabePoseUtilities.GetDistanceToBody(aimPose) / _refDistance;
						attachTransform.localPosition = 
							new Vector3(_initialLocalAttach.position.x, _initialLocalAttach.position.y, _initialLocalAttach.position.z * distanceRatio);
					}
				}
			}

			// Use the Device Pose Sources to calculate the attach transform's pose
			if (DevicePoseSource != null && DevicePoseSource.TryGetPose(out var devicePose))
			{
				attachTransform.rotation = devicePose.rotation;
			}
		}
	}

	public static class AstrolabePoseUtilities
	{
		/// <summary>
		/// Returns an estimated distance from the provided pose to the user's body.
		/// </summary>
		/// <remarks>
		/// The body is treated as a ray, parallel to the y-axis, where the start is head position.
		/// This means that moving your hand down such that is the same distance from the body will
		/// not cause the manipulated object to move further away from your hand. However, when you
		/// move your hand upward, away from your head, the manipulated object will be pushed away.
		///
		/// Internal for now, may be made public later.
		/// </remarks>
		internal static float GetDistanceToBody(Pose pose)
		{
			if (pose.position.y > Camera.main.transform.position.y)
			{
				return Vector3.Distance(pose.position, Camera.main.transform.position);
			}
			else
			{
				var headPosXZ = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
				var pointerPosXZ = new Vector2(pose.position.x, pose.position.z);

				return Vector2.Distance(pointerPosXZ, headPosXZ);
			}
		}
	}
}