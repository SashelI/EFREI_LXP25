#if METAQUEST
using static UnityEngine.XR.OpenXR.Features.Interactions.MetaQuestTouchPlusControllerProfile;
using static UnityEngine.XR.OpenXR.Features.Interactions.MetaQuestTouchProControllerProfile;
#endif
using System.Linq;
using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.OpenXR.Features.Interactions;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Tools.Helpers
{
	/// <summary>
	/// Helper class to manage controllers (Quest and/or MRTK)
	/// </summary>
	public class ControllerHelper : MonoBehaviour
	{
		#region EVENTS

		public UnityEvent<ControllerTypes, Handedness> controllerSourceDetected;
		public UnityEvent<ControllerTypes, Handedness> controllerSourceLost;
		public UnityEvent<ControllerTypes, Handedness, bool> controllerMenuButtonClicked;
		public UnityEvent<ControllerTypes, Handedness> controllerTriggerClicked;

		#endregion EVENTS

		#region PRIVATE

		private XRController _leftController;
		private XRController _rightController;
		private TrackedDevice _magicLeapController; //using TrackedDevice type because in simulated controller (editor) type isn't part of XRController class

		private Handedness _mlControllerHandedness=Handedness.None;

		/// <summary>
		/// True if the tracking source for the controllers (not hands) is currently detected
		/// </summary>
		private bool _isLeftControllerSourceTracked;
		private bool _isRightControllerSourceTracked;

		#endregion PRIVATE

		public enum ControllerTypes
		{
			None,
			MagicLeap,
			QuestTouch,
		}

		private static ControllerHelper _instance;

		public static ControllerHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<ControllerHelper>();
					if (_instance == null)
					{
						_instance = GameObject.Find("ApplicationManager").AddComponent<ControllerHelper>();
					}
				}
				return _instance;
			}
		}

		#region Monobehavior

		private void Start()
		{
			var astrolabeRayInteractors = FindObjectsOfType<AstrolabeRayInteractor>();

			foreach(var interactor in astrolabeRayInteractors)
			{
				interactor.ControllerDetected.AddListener(SetControllerDetected);
				interactor.ControllerLost.AddListener(SetControllerLost);
			}
		}

		private void Update()
		{
#if UNITY_ANDROID && METAQUEST
			if (OVRInput.GetUp(OVRInput.Button.Start))
			{
				Debug.Log($"QUEST BUTTON OUI");
				//On regarde si le bouton start a été pressé, puis relâché dans la frame courante.
				//Le bouton start n'est présent que sur la manette gauche.
				controllerMenuButtonClicked?.Invoke(ControllerTypes.QuestTouch, Handedness.Left, false);
			}
#endif
		}

#endregion Monobehavior

		#region Getters/Setters

		private void SetControllerDetected(XRController controller, InteractorHandedness handedness)
		{
			//Storing the interactor source, wether it's a hand or a controller
			if (handedness == InteractorHandedness.Left)
			{
				_leftController = controller;
			}
			else if (handedness == InteractorHandedness.Right)
			{
				_rightController = controller;
			}

			//If it's a controller, save the info
			switch (controller) 
			{
#if METAQUEST
				case QuestTouchPlusController:
				case QuestProTouchController:
					if(handedness == InteractorHandedness.Left)
					{
						_isLeftControllerSourceTracked = true;
						PointerDragHandler.Instance.SetControllerTracked(true, Handedness.Left);
						controllerSourceDetected?.Invoke(ControllerTypes.QuestTouch, Handedness.Left);
					}
					else if(handedness == InteractorHandedness.Right)
					{
						_isRightControllerSourceTracked = true;
						PointerDragHandler.Instance.SetControllerTracked(true, Handedness.Right);
						controllerSourceDetected?.Invoke(ControllerTypes.QuestTouch, Handedness.Right);
					}
					else
					{
						controllerSourceDetected?.Invoke(ControllerTypes.QuestTouch, Handedness.None);
					}
					break;
#endif
			}
		}

		private void SetControllerLost(XRController controller, InteractorHandedness handedness)
		{
			//Releasing the interactor source, wether it's a hand or a controller
			if (handedness == InteractorHandedness.Left)
			{
				_leftController = null;
			}
			else if (handedness == InteractorHandedness.Right)
			{
				_rightController = null;
			}

			//If it's a controller, save the info
			switch (controller)
			{
#if METAQUEST
				case QuestTouchPlusController:
				case QuestProTouchController:
					if (handedness == InteractorHandedness.Left)
					{
						_isLeftControllerSourceTracked = false;
						PointerDragHandler.Instance.SetControllerTracked(false, Handedness.Left);
						controllerSourceLost?.Invoke(ControllerTypes.QuestTouch, Handedness.Left);
					}
					else if (handedness == InteractorHandedness.Right)
					{
						_isRightControllerSourceTracked = false;
						PointerDragHandler.Instance.SetControllerTracked(false, Handedness.Right);
						controllerSourceLost?.Invoke(ControllerTypes.QuestTouch, Handedness.Right);
					}

					break;
#endif

			}
		}

		/// <summary>
		/// Gets MRTK controllers (even for Quest devices)
		/// </summary>
		/// <param name="controllerHandedness"></param>
		/// <param name="controller"></param>
		public void GetController(Handedness controllerHandedness, out XRController controller)
		{
			controller = controllerHandedness switch
			{
				Handedness.Left => _leftController,
				Handedness.Right => _rightController,
				_ => null
			};
		}

		/// <summary>
		/// Gets hand or controller position for non-Quest devices
		/// For controllers, returns the pointer origin pose
		/// </summary>
		/// <param name="handedness"></param>
		/// <param name="jointPos"></param>
		/// <returns></returns>
		public bool GetIndexJointPosition(Handedness handedness, out Vector3 jointPos)
		{
			jointPos = Vector3.zero;

#if METAQUEST
			InteractorBehaviorControls.Instance.GetInteractorForHand(handedness, out var rayInteractor, out _);
#elif MAGICLEAP
			InteractorBehaviorControls.Instance.GetInteractorForController(handedness, out var rayInteractor);
#endif

			if (handedness == Handedness.Left)
			{
				if (_leftController != null && _isLeftControllerSourceTracked)
				{
#if METAQUEST
					if (rayInteractor != null)
					{
						jointPos = rayInteractor.rayOriginTransform.position;
						return true;
					}
#endif
				}
#if MAGICLEAP
				if(_magicLeapController != null && _isLeftControllerSourceTracked)
				{
					if (rayInteractor != null)
					{
						jointPos = rayInteractor.rayOriginTransform.position;
						return true;
					}
				}
#endif
				else
				{
					HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out var jointPose);
					jointPos = jointPose.Position;
					return true;
				}
			}
			else if (handedness == Handedness.Right)
			{
				if (_rightController != null && _isRightControllerSourceTracked)
				{
#if METAQUEST
					if (rayInteractor != null)
					{
						jointPos = rayInteractor.rayOriginTransform.position;
						return true;
					}
#endif
				}
#if MAGICLEAP
				if (_magicLeapController != null && _isRightControllerSourceTracked)
				{
					if (rayInteractor != null)
					{
						jointPos = rayInteractor.rayOriginTransform.position;
						return true;
					}
				}
#endif
				{
					HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out var jointPose);
					jointPos = jointPose.Position;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// For devices that use only one controller, returns the hand in which it is currently held
		/// </summary>
		public Handedness GetSingleControllerHandedness(bool refresh=true)
		{

#if MAGICLEAP
			if (refresh) 
			{
				RefreshSingleHandedControllerHandedness();
			}

			if (_mlControllerHandedness != Handedness.None)
			{
				return _mlControllerHandedness;
			}
#endif
			if (IsControllerTracked(Handedness.None))
			{
				if (_leftController == null && _rightController == null)
				{
					return Handedness.Both;
				}
				if (_rightController == null )
				{
					return Handedness.Right;
				}
				if (_leftController == null)
				{
					return Handedness.Left;
				}
			}

			return Handedness.None;
		}

#endregion Getters/Setters

		#region Checkers

		/// <summary>
		/// Returns true if the left, right, both, or any controllers (not hands) are currently tracked
		/// </summary>
		/// <param name="handedness"></param>
		/// <returns></returns>
		public bool IsControllerTracked(Handedness handedness)
		{
			if (handedness == Handedness.Left)
			{
				if (_isLeftControllerSourceTracked)
				{
					if (_leftController != null && _leftController is not HandInteractionProfile.HandInteraction)
					{
						return _leftController.trackingState.value > 0;
					}
					else if (_magicLeapController != null && _mlControllerHandedness == Handedness.Left) //ML Controller in left hand
					{
						return _magicLeapController.trackingState.value > 0;
					}
				}
			}
			if (handedness == Handedness.Right)
			{
				if (_isRightControllerSourceTracked)
				{
					if (_rightController != null && _rightController is not HandInteractionProfile.HandInteraction)
					{
						return _rightController.trackingState.value > 0;
					}
					else if (_magicLeapController != null && _mlControllerHandedness == Handedness.Right) //ML Controller in right hand
					{
						return _magicLeapController.trackingState.value > 0;
					}
				}
			}

			if (handedness == Handedness.Both)
			{
				if (_rightController != null && _leftController != null)
				{
					if (_isLeftControllerSourceTracked && _leftController is not HandInteractionProfile.HandInteraction &&
					    _isRightControllerSourceTracked && _rightController is not HandInteractionProfile.HandInteraction)
					{
						return _leftController.trackingState.value > 0 && _rightController.trackingState.value > 0;
					}
				}
				else if (_magicLeapController != null) //ML Controller in any hand
				{
					return _magicLeapController.trackingState.value > 0;
				}
			}

			if (handedness == Handedness.None) //Single-handed controller
			{
				if (_magicLeapController != null)
				{
					return _magicLeapController.trackingState.value > 0;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns true if the left, right, both, hands are currently tracked
		/// </summary>
		/// <param name="handedness"></param>
		/// <returns></returns>
		public bool IsHandTracked(Handedness handedness)
		{
			if (handedness == Handedness.Left || handedness == Handedness.Both)
			{
				if ((_leftController is HandInteractionProfile.HandInteraction or MicrosoftHandInteraction.HoloLensHand or XRSimulatedController))
				{
					return true;
				}
			}
			if (handedness == Handedness.Right || handedness == Handedness.Both)
			{
				if ((_rightController is HandInteractionProfile.HandInteraction or MicrosoftHandInteraction.HoloLensHand or XRSimulatedController))
				{
					return true;
				}
			}

			if (handedness == Handedness.Both)
			{
				if ((_leftController is HandInteractionProfile.HandInteraction or MicrosoftHandInteraction.HoloLensHand or XRSimulatedController) 
				    && (_rightController is HandInteractionProfile.HandInteraction or MicrosoftHandInteraction.HoloLensHand or XRSimulatedController))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the requested button is currently pressed on any supported controller.
		/// Currently supports Menu (start) button and Index Trigger
		/// </summary>
		/// <param name="button"></param>
		/// <param name="controllerType">Controller type if button is pressed. None if not pressed.</param>
		/// <param name="handedness"></param>
		/// <returns>True if pressed</returns>
		public bool IsButtonPressed(ControllerButtons button, Handedness handedness, out ControllerTypes controllerType)
		{
#if METAQUEST
			TwinkleControllerButtonToQuestButton(button, out OVRInput.Button questButton, out OVRInput.Axis1D questTrigger);

			OVRInput.Controller requestedController = handedness switch
			{
				Handedness.Right => OVRInput.Controller.RTouch,
				Handedness.Left => OVRInput.Controller.LTouch,
				Handedness.Both => OVRInput.Controller.All,
				_ => OVRInput.Controller.Active
			};

			if (questButton != OVRInput.Button.None)
			{
				if (OVRInput.Get(questButton, requestedController))
				{
					Debug.Log($"QUEST BUTTON {OVRInput.Get(questButton, requestedController)}");
					controllerType = ControllerTypes.QuestTouch;
					return true;
				}
			}
			else if (questTrigger != OVRInput.Axis1D.None)
			{
				if (OVRInput.Get(questTrigger, requestedController) > 0.9f)
				{
					Debug.Log($"QUEST TRIGGER {OVRInput.Get(questTrigger, requestedController)}");
					controllerType = ControllerTypes.QuestTouch;
					return true;
				}
			}
#endif
			controllerType = ControllerTypes.None;
			return false;
		}

		/// <summary>
		/// Checks if the requested button was pressed on the current frame on any supported controller.
		/// </summary>
		/// <param name="button"></param>
		/// <param name="controllerType">Controller type if button is pressed. None if not pressed.</param>
		/// <param name="handedness">None if not pressed</param>
		/// <returns>True if pressed on this frame only</returns>
		public bool IsButtonPressedThisFrame(ControllerButtons button, Handedness handedness, out ControllerTypes controllerType)
		{

#if METAQUEST
			TwinkleControllerButtonToQuestButton(button, out OVRInput.Button questButton, out OVRInput.Axis1D questTrigger);

			OVRInput.Controller requestedController = handedness switch
			{
				Handedness.Right => OVRInput.Controller.RTouch,
				Handedness.Left => OVRInput.Controller.LTouch,
				Handedness.Both => OVRInput.Controller.All,
				_ => OVRInput.Controller.Active
			};

			if (questButton != OVRInput.Button.None)
			{
				Debug.Log($"QUEST BUTTON FRAME {OVRInput.GetDown(questButton, requestedController)}");
				if (OVRInput.GetDown(questButton, requestedController))
				{
					controllerType = ControllerTypes.QuestTouch;
					return true;
				}
			}
			else if (questTrigger != OVRInput.Axis1D.None)
			{
				if (OVRInput.Get(questTrigger, requestedController) > 0.9f)
				{
					controllerType = ControllerTypes.QuestTouch;
					return true;
				}
			}
#endif
			controllerType = ControllerTypes.None;
			return false;
		}

		/// <summary>
		/// For devices that use only one controller, returns the hand in which it is currently held
		/// </summary>
		public void RefreshSingleHandedControllerHandedness(bool notify=false)
		{

#if MAGICLEAP
			Handedness newHandedness=Handedness.None;

			if (IsControllerTracked(Handedness.None))
			{
				if (_leftController is not HandInteractionProfile.HandInteraction 
				    && _rightController is not HandInteractionProfile.HandInteraction)
				{
					newHandedness = Handedness.Both;
					_isLeftControllerSourceTracked = true;
					_isRightControllerSourceTracked = true;
				}
				else if (_rightController is not HandInteractionProfile.HandInteraction)
				{
					newHandedness = Handedness.Right;
					_isRightControllerSourceTracked = true;
					_isLeftControllerSourceTracked = false;
				}
				else if (_leftController is not HandInteractionProfile.HandInteraction)
				{
					newHandedness = Handedness.Left;
					_isLeftControllerSourceTracked = true;
					_isRightControllerSourceTracked = false;
				}

				if (newHandedness != _mlControllerHandedness) //We switched hands, need to notify
				{
					if (newHandedness == Handedness.Both)
					{
						ToggleHandRayWhenController(false, false);
					}
					else
					{
						ToggleHandRayWhenController(newHandedness != Handedness.Left, newHandedness != Handedness.Right);
					}

					if (notify)
					{
						var oldHandedness = _mlControllerHandedness;
						_mlControllerHandedness = newHandedness;
						controllerSourceLost?.Invoke(ControllerTypes.MagicLeap, oldHandedness);
						controllerSourceDetected?.Invoke(ControllerTypes.MagicLeap, newHandedness);
					}
				}
			}
			else
			{
				_mlControllerHandedness = Handedness.None;
			}
#endif
		}

#endregion Checkers

		#region Event Actions

		/// <summary>
		/// MagicLeap menu button is pressed
		/// </summary>
		private void MagicLeap_Menu_performed(InputAction.CallbackContext context)
		{
			var device = context.control.device;
			if (device.usages.Contains(CommonUsages.LeftHand))
			{
				controllerMenuButtonClicked?.Invoke(ControllerTypes.MagicLeap, Handedness.Left, false);
			}
			else
			{
				controllerMenuButtonClicked?.Invoke(ControllerTypes.MagicLeap, Handedness.Right, false);
			}
		}

		private void Magicleap_Trigger_performed(InputAction.CallbackContext context)
		{
			Debug.Log($"ML TRIGGER");
			var device = context.control.device;
			if (device.usages.Contains(CommonUsages.LeftHand))
			{
				controllerTriggerClicked?.Invoke(ControllerTypes.MagicLeap, Handedness.Left);
			}
			else
			{
				controllerTriggerClicked?.Invoke(ControllerTypes.MagicLeap, Handedness.Right);
			}
		}

		#endregion Event Actions

		#region Utilitary

#if METAQUEST
		/// <summary>
		/// Translates astrolabe controller button into the quest input (for mapping)
		/// On Quest, triggers are of a different type than buttons
		/// </summary>
		/// <param name="twinkleButton"></param>
		/// <param name="buttonOnDevice">Button of device type. None if request is a trigger.</param>
		/// <param name="triggerOnDevice">Trigger of device type. None if requets is a button.</param>
		private void TwinkleControllerButtonToQuestButton(ControllerButtons twinkleButton, out OVRInput.Button buttonOnDevice, out OVRInput.Axis1D triggerOnDevice)
		{
			buttonOnDevice = twinkleButton switch
			{
				ControllerButtons.Menu => OVRInput.Button.Start,
				ControllerButtons.A => OVRInput.Button.One,
				ControllerButtons.B => OVRInput.Button.Two,
				ControllerButtons.X => OVRInput.Button.Three,
				ControllerButtons.Y => OVRInput.Button.Four,
				ControllerButtons.Joystick => OVRInput.Button.PrimaryThumbstick,
				_ => OVRInput.Button.None
			};
			if (buttonOnDevice == OVRInput.Button.None)
			{
				triggerOnDevice = twinkleButton switch
				{
					ControllerButtons.Trigger => OVRInput.Axis1D.PrimaryIndexTrigger,
					ControllerButtons.Bumper => OVRInput.Axis1D.PrimaryHandTrigger,
					_ => OVRInput.Axis1D.None
				};
			}
			else
			{
				triggerOnDevice = OVRInput.Axis1D.None;
			}
		}
#endif

#if MAGICLEAP
		private void ToggleHandRayWhenController(bool activeLeft, bool activeRight)
		{
			InteractorBehaviorControls.Instance.GetInteractorForHand(Handedness.Left, out var interactorLeft, out _);
			InteractorBehaviorControls.Instance.GetInteractorForHand(Handedness.Right, out var interactorRight, out _);

			InteractorBehaviorControls.Instance.ToggleInteractors(new XRBaseInteractor[] { interactorLeft }, activeLeft);
			InteractorBehaviorControls.Instance.ToggleInteractors(new XRBaseInteractor[] { interactorRight }, activeRight);
		}
#endif

		#endregion Utilitary
	}
}