using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class HandJointUtils
	{
		private static HandsAggregatorSubsystem _aggregator;

		private static AstrolabeRiggedHandMeshVisualizer _leftHandRig;
		private static AstrolabeRiggedHandMeshVisualizer _rightHandRig;

		private static void Init()
		{
			_aggregator = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

			var riggedHands = Object.FindObjectsOfType<AstrolabeRiggedHandMeshVisualizer>();

			foreach (var rig in riggedHands)
			{
				if (rig.HandNode == XRNode.LeftHand)
				{
					_leftHandRig = rig;
				}
				else if (rig.HandNode == XRNode.RightHand)
				{
					_rightHandRig = rig;
				}
			}
		}

		public static XRNode GetHandNodeFromHandedness(Handedness handedness)
		{
			return handedness switch
			{
				Handedness.Left => XRNode.LeftHand,
				Handedness.Right => XRNode.RightHand,
				_ => XRNode.CenterEye
			};
		}

		/// <summary>
		/// Tries to get the pose of the requested joint for the first controller with the specified handedness.
		/// </summary>
		/// <param name="joint">The requested joint</param>
		/// <param name="handedness">The specific hand of interest. This should be either Handedness.Left or Handedness.Right</param>
		/// <param name="pose">The output pose data</param>
		public static bool TryGetJointPose(TrackedHandJoint joint, Handedness handedness, out HandJointPose pose)
		{
			if (_aggregator == null)
			{
				Init();
			}

			if (_aggregator != null && handedness != Handedness.None)
			{
				return _aggregator.TryGetJoint(joint, GetHandNodeFromHandedness(handedness), out pose);
			}

			pose = new HandJointPose();
			return false;
		}

		public static bool TryGetJointPoseForAvatar(TrackedHandJoint joint, Handedness handedness, out Pose pose)
		{
			switch (handedness)
			{
				case Handedness.Left:
					if (_leftHandRig == null)
					{
						Init();
					}

					if (_leftHandRig != null)
					{
						return _leftHandRig.TryGetHandJoint(joint, out pose);
					}

					break;

				case Handedness.Right:
					if (_rightHandRig == null)
					{
						Init();
					}

					if (_rightHandRig != null)
					{
						return _rightHandRig.TryGetHandJoint(joint, out pose);
					}

					break;
			}

			pose = new HandJointPose();
			return false;
		}

		/// <summary>
		/// Find the first detected hand controller with matching handedness.
		/// </summary>
		/// <remarks>
		/// The given handedness should be either Handedness.Left or Handedness.Right.
		/// </remarks>
		public static bool FindHandFromNode(Handedness handedness, out InputDevice hand)
		{
			hand = default;

			if (handedness != Handedness.None)
			{
				hand = InputDevices.GetDeviceAtXRNode(GetHandNodeFromHandedness(handedness));
			}

			return hand != null && hand.isValid && hand.characteristics == InputDeviceCharacteristics.HandTracking;
		}

		/// <summary>
		/// Scans the scene for XRControllers and returns true if the tracked controller corresponding to the handedness is a hand.
		/// </summary>
		/// <returns></returns>
		public static bool FindHandFromScene(Handedness handedness) //TODO
		{
			return TryGetPinch(handedness, out _, out _, out _);
		}

		public static bool TryGetPinch(Handedness handedness, out bool isReadyToPinch, out bool isPinching, out float pinchAmount)
		{
			if (_aggregator == null)
			{
				Init();
			}

			if (_aggregator != null && handedness != Handedness.None)
			{
				return _aggregator.TryGetPinchProgress(GetHandNodeFromHandedness(handedness), out isReadyToPinch, out isPinching, out pinchAmount);
			}
			else
			{
				isReadyToPinch = false;
				pinchAmount = 0;
				isPinching = false;
				return false;
			}
		}

		public static void ActivateRiggedHandJointUpdate(bool enable = true)
		{
			if (_leftHandRig == null || _rightHandRig == null)
			{
				Init();
			}

			if (_leftHandRig != null)
			{
				_leftHandRig.UpdateJointsWhenNotRendered = enable;
			}
			if (_rightHandRig != null)
			{
				_rightHandRig.UpdateJointsWhenNotRendered = enable;
			}
		}
	}
}