using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	/// <summary>
	/// MRTK Fallback composite pose source override to handle Quest hands, quest controllers, HL hands correctly with OpenXR
	/// </summary>
	[Serializable]
	public class AstrolabeFallbackPoseSource : IPoseSource, ISerializationCallbackReceiver
	{
		[SerializeReference] [InterfaceSelector] [Tooltip("An ordered list of pose sources to query.")]
		private IPoseSource[] poseSourceList;

		[SerializeField] [Tooltip("An ordered list of pose sources to query.")] [Obsolete] [HideInInspector]
		private PoseSourceWrapper[] poseSources;

		/// <summary>
		/// An ordered list of pose sources to query.
		/// </summary>
		protected IPoseSource[] PoseSources
		{
			get => poseSourceList;
			set => poseSourceList = value;
		}

		public bool IsController { get; set; } = false;

		/// <summary>
		/// Tries to get a pose from each pose source in order, returning the result of the first pose source
		/// which returns a success.
		/// </summary>
		public bool TryGetPose(out Pose pose)
		{
			for (var i = 0; i < poseSourceList.Length; i++)
			{
				var currentPoseSource = poseSourceList[i];
				if (currentPoseSource != null && currentPoseSource.TryGetPose(out pose))
				{
#if UNITY_ANDROID && !UNITY_EDITOR
					//We want to use polyfill source when using Quest hands, but InputAction works better with meta controllers.
					if (currentPoseSource is InputActionPoseSource && !IsController)
					{
						continue;
					}
#endif
					return true;
				}
			}

			pose = Pose.identity;
			return false;
		}

		[Obsolete]
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (poseSources != null && poseSources.Length > 0)
			{
				poseSourceList = new IPoseSource[poseSources.Length];

				for (var i = 0; i < poseSources.Length; i++)
				{
					var poseSource = poseSources[i];
					poseSourceList[i] = poseSource.source;
				}

				poseSources = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		[Serializable]
		[Obsolete]
		private struct PoseSourceWrapper
		{
			[SerializeReference] [InterfaceSelector] [Tooltip("The pose source we are trying to get the pose of")]
			public IPoseSource source;
		}
	}
}