using System;
using Astrolabe.Diagnostics;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Scripts.Tools
{
	public class GlobalClickEventChannel
	{
		private static readonly GlobalClickEventChannel _instance = new GlobalClickEventChannel();

		/// <summary>
		/// This event is raised when an empty space is clicked
		/// </summary>
		public event Action VoidClicked;

		/// <summary>
		/// This event is raised when a target is clicked when measureMode
		/// </summary>
		public event Action<Vector3, Vector3> MeasureClicked;

		/// <summary>
		/// This event is raised when a target is clicked when primitive shape mode
		/// </summary>
		public event Action<Vector3, GameObject> IconClicked;

		/// <summary>
		/// Raised when a pointer is down (pinch)
		/// </summary>
		public event Action<XRRayInteractor, bool> RaySelectDown;

		/// <summary>
		/// Raised when a pointer is up (pinch released)
		/// </summary>
		public event Action<XRRayInteractor, bool> RaySelectUp;

		/// <summary>
		/// Raised when a pinch is detected (useful when there is no Pointer active, cf AirDraw)
		/// </summary>
		public event Action<IXRSelectInteractor, bool> SelectPinchDown;
		/// <summary>
		/// Raised when a pinch is released (useful when there is no Pointer active, cf AirDraw)
		/// </summary>
		public event Action<IXRSelectInteractor, bool> SelectPinchUp;

		static GlobalClickEventChannel()
		{
		}

		public static GlobalClickEventChannel Instance
		{
			get
			{
				return _instance;
			}
		}

		public void RaiseOnVoidClickedEvent()
		{
			if (VoidClicked != null)
			{
				VoidClicked.Invoke();
			}
			else
			{
				Log.WriteLine("No handler registered for UI event ExcludedTargetVoidClicked.");
			}
		}


		/// <param name="interactor"></param>
		/// <param name="isHand">Is the controller a hand</param>
		public void RaiseOnRaySelectDownEvent(XRRayInteractor interactor, bool isHand)
		{
			RaySelectDown?.Invoke(interactor, isHand);
		}

		/// <param name="interactor"></param>
		/// <param name="isHand">Is the controller a hand</param>
		public void RaiseOnRaySelectUpEvent(XRRayInteractor interactor, bool isHand)
		{
			RaySelectUp?.Invoke(interactor, isHand);
		}

		/// <param name="interactor"></param>
		/// <param name="isHand">Is the controller a hand</param>
		public void RaiseOnSelectPinchDownEvent(IXRSelectInteractor interactor, bool isHand)
		{
			SelectPinchDown?.Invoke(interactor, isHand);
		}

		/// <param name="interactor"></param>
		/// <param name="isHand">Is the controller a hand</param>
		public void RaiseOnSelectPinchUpEvent(IXRSelectInteractor interactor, bool isHand)
		{
			SelectPinchUp?.Invoke(interactor, isHand);
		}
	}
}