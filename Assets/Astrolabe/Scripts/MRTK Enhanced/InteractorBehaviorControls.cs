// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	/// <summary>
	/// Example class to demonstrate how to turn various interactors on and off.
	/// </summary>
	/// <remarks>
	/// Hook up buttons to the public functions to turn interactors on and off.
	/// </remarks>
	[AddComponentMenu("MRTK/Examples/InteractorBehaviorControls")]
	public class InteractorBehaviorControls : MonoBehaviour
	{
		[SerializeField]
		private InteractionModeManager interactionModeManager;

		[SerializeField]
		private XRInteractionManager interactionManager;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] handRaysInteractors;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] controllerRayInteractors;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] grabInteractors;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] pokeInteractors;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] gazePinchInteractors;

		[SerializeField]
		private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor gazeInteractor;

		/// <summary>
		/// event triggered when hand rays are toggled on or off
		/// </summary>
		public event Action<bool> onHandRayToggled;

		/// <summary>
		/// event triggered when controller rays are toggled on or off
		/// </summary>
		public event Action<bool> onControllerRayToggled;

		/// <summary>
		/// event triggered when grab interactors are toggled on/off
		/// </summary>
		public event Action<bool> onGrabToggled;

		/// <summary>
		/// event triggered when poke interactors are toggled on/off
		/// </summary>
		public event Action<bool> onPokeToggled;

		/// <summary>
		/// event triggered when gaze interactors are toggled on/off
		/// </summary>
		public event Action<bool> onGazeToggled;

		private static InteractorBehaviorControls _instance;

		public static InteractorBehaviorControls Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<InteractorBehaviorControls>();

					if (_instance == null)
					{
						_instance = GameObject.Find("ApplicationManager").AddComponent<InteractorBehaviorControls>();
					}
				}

				return _instance;
			}
		}

		/// <summary>
		/// Enable all interactors
		/// </summary>
		public void EnableAll()
		{
			SetControllerRayActive(true);
			SetHandGrabActive(true);
			SetHandPokeActive(true);
			SetHandRayActive(true);
			SetGazeActive(true);
			SetGazePinchActive(true);
		}

		/// <summary>
		/// Enable everything, and disable all gaze interactions.
		/// </summary>
		public void OnlyHands()
		{
			EnableAll();
			SetGazeActive(false);
			SetGazePinchActive(false);
		}

		/// <summary>
		/// Enable everything, and disable all hand interactions.
		/// </summary>
		public void OnlyGaze()
		{
			EnableAll();
			SetControllerRayActive(false);
			SetHandGrabActive(false);
			SetHandPokeActive(false);
			SetHandRayActive(false);
		}

		/// <summary>
		/// Enable everything, and disable controller ray interactors.
		/// </summary>
		public void DisableControllerRays()
		{
			EnableAll();
			SetControllerRayActive(false);
		}

		/// <summary>
		/// Enable everything, and disable hand grab interactors.
		/// </summary>
		public void DisableHandGrabs()
		{
			EnableAll();
			SetHandGrabActive(false);
		}

		/// <summary>
		/// Enable everything, and disable hand poke interactors.
		/// </summary>
		public void DisableHandPokes()
		{
			EnableAll();
			SetHandPokeActive(false);
		}

		/// <summary>
		/// Enable everything, and disable hand poke interactors.
		/// </summary>
		public void DisableHandRays()
		{
			EnableAll();
			SetHandRayActive(false);
		}

		/// <summary>
		/// Enable everything, and disable gaze interactors.
		/// </summary>
		public void DisableGaze()
		{
			EnableAll();
			SetGazeActive(false);
		}

		/// <summary>
		/// Enable everything, and disable gaze pinch interactors.
		/// </summary>
		public void DisableGazePinch()
		{
			EnableAll();
			SetGazePinchActive(false);
		}

		/// <summary>
		/// Enable or disable the specified gaze interactors.
		/// </summary>
		public void SetGazeActive(bool isActive)
		{
			if (ToggleInteractor(gazeInteractor, isActive))
			{
				onGazeToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enable or disable the specified gaze pinch interactors.
		/// </summary>
		public void SetGazePinchActive(bool isActive)
		{
			if (ToggleInteractors(gazePinchInteractors, isActive))
			{
				onGazeToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enable or disable the specified poke interactors.
		/// </summary>
		public void SetHandPokeActive(bool isActive)
		{
			if (ToggleInteractors(pokeInteractors, isActive))
			{
				onPokeToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enable or disable the specified hand grab interactors.
		/// </summary>
		public void SetHandGrabActive(bool isActive)
		{
			if (ToggleInteractors(grabInteractors, isActive))
			{
				onGrabToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enable or disable the specified controller ray interactors.
		/// </summary>
		public void SetControllerRayActive(bool isActive)
		{
			if (ToggleInteractors(controllerRayInteractors, isActive))
			{
				onControllerRayToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enable or disable the specified hand ray interactors.
		/// </summary>
		public void SetHandRayActive(bool isActive)
		{
			if (ToggleInteractors(handRaysInteractors, isActive))
			{
				onHandRayToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Enables or disables the given hand or controller ray interactors.
		/// </summary>
		public void SetSpecificRaysActive(XRBaseInteractor[] interactors, bool isActive)
		{
			if (ToggleInteractors(interactors, isActive))
			{
				onHandRayToggled?.Invoke(isActive);
			}
		}

		/// <summary>
		/// Toggle interactors, and return true if something changed.
		/// </summary>
		public bool ToggleInteractors(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] interactors, bool isActive)
		{
			if (isActive)
			{
				return ActivateInteractors(interactors);
			}
			else
			{
				return DeactivateInteractors(interactors);
			}
		}

		/// <summary>
		/// Toggle interactor, and return true if something changed.
		/// </summary>
		private bool ToggleInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor, bool isActive)
		{
			if (isActive)
			{
				return ActivateInteractor(interactor);
			}
			else
			{
				return DeactivateInteractor(interactor);
			}
		}

		/// <summary>
		/// Activate interactors, and return true if something changed.
		/// </summary>
		private bool ActivateInteractors(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] interactors)
		{
			bool change = false;
			for (int i = 0; i < interactors.Length; i++)
			{
				change |= ActivateInteractor(interactors[i]);
			}
			return change;
		}

		/// <summary>
		/// Activate interactor, and return true if something changed.
		/// </summary>
		private bool ActivateInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
		{
			if (interactor.gameObject.activeSelf)
			{
				return false;
			}

			interactor.gameObject.SetActive(true);
			interactionModeManager.RegisterInteractor(interactor);
			return true;
		}

		/// <summary>
		/// Deactivate interactors, and return true if something changed.
		/// </summary>
		private bool DeactivateInteractors(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] interactors)
		{
			bool change = false;
			for (int i = 0; i < interactors.Length; i++)
			{
				change |= DeactivateInteractor(interactors[i]);
			}
			return change;
		}

		/// <summary>
		/// Deactivate interactor, and return true if something changed.
		/// </summary>
		private bool DeactivateInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
		{
			if (!interactor.gameObject.activeSelf)
			{
				return false;
			}

			interactionModeManager.UnregisterInteractor(interactor);
			interactor.gameObject.SetActive(false);
			return true;
		}

		/// <summary>
		/// Returns associated interactors for left or right hand.
		/// TODO Add grab, gaze etc. out if needed
		/// </summary>
		/// <param name="handedness"></param>
		/// <param name="rayInteractor"></param>
		/// <param name="pokeInteractor"></param>
		/// <returns></returns>
		public void GetInteractorForHand(Handedness handedness, out AstrolabeRayInteractor rayInteractor, out PokeInteractor pokeInteractor)
		{
			rayInteractor = null;
			pokeInteractor = null;

			if (handRaysInteractors.Length > 0)
			{
				if (handRaysInteractors.Length == 1)
				{
					rayInteractor = handRaysInteractors[0] as AstrolabeRayInteractor;
				}
				else
				{
					rayInteractor = handedness==Handedness.Left ? handRaysInteractors[1] as AstrolabeRayInteractor : handRaysInteractors[0] as AstrolabeRayInteractor;
				}
			}

			if (pokeInteractors.Length > 0)
			{
				if (pokeInteractors.Length == 1)
				{
					pokeInteractor = pokeInteractors[0] as PokeInteractor;
				}
				else
				{
					pokeInteractor = handedness == Handedness.Left ? pokeInteractors[1] as PokeInteractor : pokeInteractors[0] as PokeInteractor;
				}
			}
		}

		/// <summary>
		/// Returns associated interactors for left or right controller.
		/// TODO Add grab, gaze etc. out if needed
		/// </summary>
		/// <param name="handedness"></param>
		/// <param name="rayInteractor"></param>
		/// <param name="pokeInteractor"></param>
		/// <returns></returns>
		public void GetInteractorForController(Handedness handedness, out AstrolabeRayInteractor rayInteractor)
		{
			rayInteractor = null;

			if (controllerRayInteractors.Length > 0)
			{
				if (controllerRayInteractors.Length == 1)
				{
					rayInteractor = controllerRayInteractors[0] as AstrolabeRayInteractor;
				}
				else
				{
					rayInteractor = handedness == Handedness.Left ? controllerRayInteractors[1] as AstrolabeRayInteractor : controllerRayInteractors[0] as AstrolabeRayInteractor;
				}
			}
		}
	}
}