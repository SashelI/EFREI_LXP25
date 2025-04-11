#if UNITY_EDITOR

using System;
using System.Reflection;
using Astrolabe.Diagnostics;
using UnityEditor;
using UnityEngine;

// This class finds and kills a coroutine that throws errors inside the editor every 5 seconds if no headset is connected
// The coroutine was introduced in the OpenXR Plugin [1.5.1] - 2022-08-11
// There absolutely has to be a better way, and this code should NOT be maintained incase the issue is resolved
namespace Assets.Astrolabe.Scripts
{
	public class OpenXRRestarterKiller : MonoBehaviour
	{
		private static bool _isHookedIntoUpdate = false;
		private static object _restarterInstance = null;

		private static Type _restarterType = null;
		private static FieldInfo _singletonInstanceField = null;
		private static FieldInfo _restarterCoroutine = null;
		private static MethodInfo _stopMethod = null;

		[InitializeOnLoadMethod]
		[ExecuteInEditMode]
		private static void Init()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange change)
		{
			switch (change)
			{
				case PlayModeStateChange.EnteredPlayMode:
					GatherReflectionData();
					SetHooked(true);
					break;

				case PlayModeStateChange.ExitingPlayMode:
					SetHooked(false);
					break;
			}
		}

		private static void GatherReflectionData()
		{
			_restarterType =
				Type.GetType(
					"UnityEngine.XR.OpenXR.OpenXRRestarter, Unity.XR.OpenXR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nul",
					true);
			_singletonInstanceField = _restarterType.GetField("s_Instance", BindingFlags.NonPublic | BindingFlags.Static);
			_restarterCoroutine =
				_restarterType.GetField("m_pauseAndRestartCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
			_stopMethod = typeof(MonoBehaviour).GetMethod("StopCoroutine", new Type[] { typeof(Coroutine) });
		}

		// Enables/Disables looking for the restarter coroutine
		private static void SetHooked(bool isHooked)
		{
			if (_isHookedIntoUpdate == isHooked)
			{
				return; // Already have the desired state
			}

			_isHookedIntoUpdate = isHooked;
			if (isHooked)
			{
				EditorApplication.update += OnUpdate;
			}
			else
			{
				EditorApplication.update -= OnUpdate;
			}
		}

		private static void OnUpdate()
		{
			// Search for the singleton instance, we run before it initializes
			if (_restarterInstance == null && _singletonInstanceField.GetValue(null) != null)
			{
				_restarterInstance = _singletonInstanceField.GetValue(null);
			}

			// Check if we have an instance with an active restarter coroutine.
			if (_restarterInstance != null && _restarterCoroutine.GetValue(_singletonInstanceField.GetValue(null)) != null)
			{
				Log.WriteLine(
					"Killing internal OpenXR restart coroutine after connection failure! This is an editor hack to avoid reoccurring reconnection errors!");
				_stopMethod.Invoke(_restarterInstance, new object[] { _restarterCoroutine.GetValue(_restarterInstance) });
				_restarterCoroutine.SetValue(_restarterInstance, null);

				// Our job is done, no reason to linger anymore
				SetHooked(false);
			}
		}
	}
}

#endif