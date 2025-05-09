using Assets.Astrolabe.Scripts.Tools;
using MixedReality.Toolkit;
using UnityEngine;

namespace Assets.Scripts.Tools
{
	/// <summary>
	/// Util class to gather HandPose data. Adapted from MRTK2.
	/// </summary>
	public static class HandPoseUtils
	{
		private const float INDEX_THUMB_SQR_MAGNITUDE_THRESHOLD = 0.0016f;

		/// <summary>
		/// Returns true if index finger tip is closer to wrist than index knuckle joint.
		/// </summary>
		/// <param name="hand">Hand to query joint pose against.</param>
		public static bool IsIndexGrabbing(Handedness hand)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, hand, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, hand, out var indexTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexProximal, hand, out var indexKnucklePose))
			{
				// compare wrist-knuckle to wrist-tip
				Vector3 wristToIndexTip = indexTipPose.Position - wristPose.Position;
				Vector3 wristToIndexKnuckle = indexKnucklePose.Position - wristPose.Position;
				return wristToIndexKnuckle.sqrMagnitude >= wristToIndexTip.sqrMagnitude;
			}
			return false;
		}


		/// <summary>
		/// Returns true if middle finger tip is closer to wrist than middle knuckle joint.
		/// </summary>
		/// <param name="hand">Hand to query joint pose against.</param>
		public static bool IsMiddleGrabbing(Handedness hand)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, hand, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, hand, out var indexTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleProximal, hand, out var indexKnucklePose))
			{
				// compare wrist-knuckle to wrist-tip
				Vector3 wristToIndexTip = indexTipPose.Position - wristPose.Position;
				Vector3 wristToIndexKnuckle = indexKnucklePose.Position - wristPose.Position;
				return wristToIndexKnuckle.sqrMagnitude >= wristToIndexTip.sqrMagnitude;
			}
			return false;
		}

		/// <summary>
		/// Returns true if middle thumb tip is closer to pinky knuckle than thumb knuckle joint.
		/// </summary>
		/// <param name="hand">Hand to query joint pose against.</param>
		public static bool IsThumbGrabbing(Handedness hand)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.LittleProximal, hand, out var pinkyKnucklePose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, hand, out var thumbTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximal, hand, out var thumbKnucklePose))
			{
				// compare pinkyKnuckle-ThumbKnuckle to pinkyKnuckle-ThumbTip
				Vector3 pinkyKnuckleToThumbTip = thumbTipPose.Position - pinkyKnucklePose.Position;
				Vector3 pinkyKnuckleToThumbKnuckle = thumbKnucklePose.Position - pinkyKnucklePose.Position;
				return pinkyKnuckleToThumbKnuckle.sqrMagnitude >= pinkyKnuckleToThumbTip.sqrMagnitude;
			}
			return false;
		}

		/*
        * Finger Curl Utils: Util Functions to calculate the curl of a specific finger. 
        * Author: Chaitanya Shah
        * github: https://github.com/chetu3319
        */
		/// <summary>
		/// Returns curl of ranging from 0 to 1. 1 if index finger curled/closer to wrist. 0 if the finger is not curled.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if index finger is straight/not curled, 1 if index finger is curled</returns>
		public static float IndexFingerCurl(Handedness handedness)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, handedness, out var fingerTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexProximal, handedness, out var fingerKnucklePose))
			{

				return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
			}
			return 0.0f;
		}

		/// <summary>
		/// Returns curl of middle finger ranging from 0 to 1. 1 if index finger curled/closer to wrist. 0 if the finger is not curled.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if middle finger is straight/not curled, 1 if middle finger is curled</returns>
		public static float MiddleFingerCurl(Handedness handedness)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, handedness, out var fingerTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleProximal, handedness, out var fingerKnucklePose))
			{
				return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
			}
			return 0.0f;
		}

		/// <summary>
		/// Returns curl of ring finger ranging from 0 to 1. 1 if ring finger curled/closer to wrist. 0 if the finger is not curled.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if ring finger is straight/not curled, 1 if ring finger is curled</returns>
		public static float RingFingerCurl(Handedness handedness)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, handedness, out var fingerTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.RingProximal, handedness, out var fingerKnucklePose))
			{
				return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
			}
			return 0.0f;
		}

		/// <summary>
		/// Returns curl of pinky finger ranging from 0 to 1. 1 if pinky finger curled/closer to wrist. 0 if the finger is not curled.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if pinky finger is straight/not curled, 1 if pinky finger is curled</returns>
		public static float PinkyFingerCurl(Handedness handedness)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, handedness, out var wristPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.LittleTip, handedness, out var fingerTipPose) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.LittleProximal, handedness, out var fingerKnucklePose))
			{
				return CalculateCurl(wristPose.Position, fingerKnucklePose.Position, fingerTipPose.Position);
			}
			return 0.0f;
		}

		/// <summary>
		/// Returns curl of thumb finger ranging from 0 to 1. 1 if thumb finger curled/closer to wrist. 0 if the finger is not curled.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if thumb finger is straight/not curled, 1 if thumb finger is curled</returns>
		public static float ThumbFingerCurl(Handedness handedness)
		{
			if (HandJointUtils.TryGetJointPose(TrackedHandJoint.LittleProximal, handedness, out var pinkyKnuckle) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, handedness, out var thumbTip) &&
				HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximal, handedness, out var thumbKnuckle))
			{
				return CalculateCurl(pinkyKnuckle.Position, thumbKnuckle.Position, thumbTip.Position);
			}
			return 0.0f;
		}

		/// <summary>
		/// Curl calculation of a finger based on the angle made by vectors wristToFingerKuncle and fingerKuckleToFingerTip.
		/// </summary>
		static private float CalculateCurl(Vector3 wristJoint, Vector3 fingerKnuckleJoint, Vector3 fingerTipJoint)
		{
			var palmToFinger = (fingerKnuckleJoint - wristJoint).normalized;
			var fingerKnuckleToTip = (fingerKnuckleJoint - fingerTipJoint).normalized;

			var curl = Vector3.Dot(fingerKnuckleToTip, palmToFinger);
			// Redefining the range from [-1,1] to [0,1]
			curl = (curl + 1) / 2.0f;
			return curl;
		}

		/// <summary>
		/// Pinch calculation of the index finger with the thumb based on the distance between the finger tip and the thumb tip.
		/// 4 cm (0.04 unity units) is the threshold for fingers being far apart and pinch being read as 0.
		/// </summary>
		/// <param name="handedness">Handedness to query joint pose against.</param>
		/// <returns> Float ranging from 0 to 1. 0 if the thumb and finger are not pinched together, 1 if thumb finger are pinched together</returns>
		public static float CalculateIndexPinch(Handedness handedness)
		{
			HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, handedness, out var indexPose);
			HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, handedness, out var thumbPose);

			Vector3 distanceVector = indexPose.Position - thumbPose.Position;
			float indexThumbSqrMagnitude = distanceVector.sqrMagnitude;

			float pinchStrength = Mathf.Clamp(1 - indexThumbSqrMagnitude / INDEX_THUMB_SQR_MAGNITUDE_THRESHOLD, 0.0f, 1.0f);
			return pinchStrength;
		}
	}
}
