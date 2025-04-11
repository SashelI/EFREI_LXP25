using System;
using System.Collections.Generic;
using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Assets.Scripts.Tools.Helpers;
using Astrolabe.Diagnostics;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Tools
{
	public class GlobalClickHandler : MonoBehaviour
	{
		[SerializeField] private AstrolabeRayInteractor leftRayInteractor;
		[SerializeField] private AstrolabeRayInteractor rightRayInteractor;

		[SerializeField] private PokeInteractor leftPokeInteractor;
		[SerializeField] private PokeInteractor rightPokeInteractor;

		[SerializeField]
		private List<GameObject> excludedTargets = new();

		[SerializeField]
		private InteractorBehaviorControls interactorBehaviorControls;

		private AstrolabeRayInteractor _controllerInteractor=null;

		private InteractorsHandler _interactorsHandler;
		private PointerClickHandler _pointerClickHandler;
		private ControllerHelper _controllerHelper;
#if MAGICLEAP
		private Handedness _lastMLControllerTriggerPressed = Handedness.None;
#endif

		public UnityEvent<Vector3, Vector3, Handedness, GameObject> VoidClicked;
		public UnityEvent<Vector3, Vector3, Handedness, GameObject> ExcludedTargetVoidClicked;

		public void GetActiveInteractors(out AstrolabeRayInteractor pointerFarLeft, out AstrolabeRayInteractor pointerFarRight, out PokeInteractor pointerNearLeft, out PokeInteractor pointerNearRight, bool refreshHandedness=true)
		{
			if(_controllerInteractor != null)
			{
				var controllerHandedness = _controllerHelper.GetSingleControllerHandedness(refreshHandedness);

				switch (controllerHandedness)
				{
					case Handedness.Left:
						pointerFarLeft = _controllerInteractor;
						pointerFarRight = rightRayInteractor;
						break;

					case Handedness.Right:
						pointerFarRight = _controllerInteractor;
						pointerFarLeft = leftRayInteractor;
						break;

					case Handedness.Both:
						pointerFarLeft = _controllerInteractor;
						pointerFarRight = _controllerInteractor;
						break;

					default:
						pointerFarLeft = leftRayInteractor;
						pointerFarRight = rightRayInteractor;
						break;
				}
			}
			else
			{
				pointerFarLeft = leftRayInteractor;
				pointerFarRight = rightRayInteractor;
			}

			pointerNearLeft = leftPokeInteractor;
			pointerNearRight = rightPokeInteractor;
		}

		public static GlobalClickHandler Instance
		{
			get
			{
				if (_instance == null)
				{
					var go = GameObject.FindGameObjectWithTag("PlacementParent");
					if (go != null)
					{
						go.WithComponent<GlobalClickHandler>(handler =>
						{
							_instance = handler;
						}, true);
					}
				}

				return _instance;
			}
		}
		private static GlobalClickHandler _instance;

		public void PointerClicked(PointerClickedEventArgs eventData)
		{
			try
			{
				// Ignore event
				if (eventData == null)
				{
					return;
				}

   				if (eventData.Target == null)
				{
					if(eventData.Pointer is GrabInteractor)
					{
						return;
					}
					Vector3 pos = Vector3.zero;

					if (eventData.Handedness == Handedness.Right)
					{
						ControllerHelper.Instance.GetIndexJointPosition(Handedness.Right, out pos);
						VoidClicked?.Invoke(pos, pos, Handedness.Right, null);
					}
					else if (eventData.Handedness == Handedness.Left)
					{
						ControllerHelper.Instance.GetIndexJointPosition(Handedness.Left, out pos);
						VoidClicked?.Invoke(pos, pos, Handedness.Left, null);
					}
#if MAGICLEAP
					else if (eventData.Handedness == Handedness.None)
					{
						CheckControllerTrigger();
						return;
					}
#endif
					GlobalClickEventChannel.Instance.RaiseOnVoidClickedEvent();
					return;
				}

				//If target isn't null
				if (excludedTargets != null)
				{
					foreach (GameObject excludedTarget in excludedTargets)
					{
						if (eventData.Target.IsParentOrChildOf(excludedTarget.transform))
						{
							ExcludedTargetVoidClicked?.Invoke(eventData.TargetHitPosition, eventData.ClickPosition, eventData.Handedness, eventData.Target.gameObject);
							return;
						}
					}
				}

				VoidClicked?.Invoke(eventData.TargetHitPosition, eventData.ClickPosition, eventData.Handedness, eventData.Target.gameObject);
			}
			catch (Exception e)
			{
				Log.WriteLine(e);
			}
		}

		public void OnPointerDown(SelectEnterEventArgs eventData)
		{
			bool isHand = eventData.interactorObject is IHandedInteractor;

			if (eventData.interactorObject is XRRayInteractor rayInteractor)
			{
				GlobalClickEventChannel.Instance.RaiseOnRaySelectDownEvent(rayInteractor, isHand);
			}
			else
			{
				GlobalClickEventChannel.Instance.RaiseOnSelectPinchDownEvent(eventData.interactorObject, isHand);
			}
		}

		public void OnPointerUp(SelectExitEventArgs eventData)
		{
			bool isHand = eventData.interactorObject is IHandedInteractor;
			bool isRightHand = eventData.interactorObject.handedness == InteractorHandedness.Right;

			if (eventData.interactorObject is XRRayInteractor rayInteractor)
			{
				GlobalClickEventChannel.Instance.RaiseOnRaySelectUpEvent(rayInteractor, isHand);
			}
			else
			{
				GlobalClickEventChannel.Instance.RaiseOnSelectPinchUpEvent(eventData.interactorObject, isHand);
			}
		}

		private void OnEnable()
		{
			if (_interactorsHandler == null)
			{
				_interactorsHandler = InteractorsHandler.Instance;
			}

			if (_pointerClickHandler == null)
			{
				_pointerClickHandler = PointerClickHandler.Instance;
			}

			if(_controllerHelper == null)
			{
				_controllerHelper = ControllerHelper.Instance;
			}

			if (_pointerClickHandler != null)
			{
				_pointerClickHandler.PointerUp += PointerClicked;
			}

			if (_controllerHelper != null)
			{
				_controllerHelper.controllerSourceDetected.AddListener(OnControllerDetected);
				_controllerHelper.controllerSourceLost.AddListener(OnControllerLost);
#if MAGICLEAP
				_controllerHelper.controllerTriggerClicked.AddListener(OnTriggerClicked);
#endif
			}
		}

#if MAGICLEAP
		/// <summary>
		/// Sets the handedness of the trigger click to avoid handedness check when "pointerClicked" event arrives
		/// </summary>
		/// <param name="type"></param>
		/// <param name="handedness"></param>
		private void OnTriggerClicked(ControllerHelper.ControllerTypes type, Handedness handedness)
		{
			_lastMLControllerTriggerPressed = handedness;
		}

		/// <summary>
		/// Because a click from ML controller is not handed, need to gather the correct handedness from the trigger_performed action
		/// </summary>
		private void CheckControllerTrigger()
		{
			if (_lastMLControllerTriggerPressed != Handedness.None)
			{
				InteractorBehaviorControls.Instance.GetInteractorForController(_lastMLControllerTriggerPressed, out var controllerRay);

				if (controllerRay != null)
				{
					controllerRay.TryGetCurrent3DRaycastHit(out var hit);
					var target = hit.transform;

					PointerClickedEventArgs clickedArgs = new(controllerRay, controllerRay.rayOriginTransform.position, target, hit.point, _lastMLControllerTriggerPressed);

					PointerClicked(clickedArgs);
				}
			}
		}
#endif

		public void SetFarPointersActive(bool active)
		{
#if MAGICLEAP
			//Considering we manage the (de)activation of hand ray if controller manually in controllerHelper, we mustn't reactivate an unwanted hand ray.
			if (_controllerHelper.IsControllerTracked(Handedness.None))
			{
				var controllerHandedness = _controllerHelper.GetSingleControllerHandedness(false);
				var handHandedness = Handedness.Left;

				if (controllerHandedness == Handedness.Left)
				{
					handHandedness = Handedness.Right;
				}

				interactorBehaviorControls.GetInteractorForController(controllerHandedness, out var controllerInteractor);
				interactorBehaviorControls.GetInteractorForHand(handHandedness, out var handInteractor, out _);

				interactorBehaviorControls.SetSpecificRaysActive(new XRBaseInteractor[]{controllerInteractor, handInteractor}, active);
			}
			else
			{
				interactorBehaviorControls.SetHandRayActive(active);
			}
#else
			interactorBehaviorControls.SetHandRayActive(active);
			interactorBehaviorControls.SetControllerRayActive(active);
#endif
		}

		public void SetNearPointersActive(bool active)
		{
			interactorBehaviorControls.SetHandPokeActive(active);
		}

		private void OnControllerDetected(ControllerHelper.ControllerTypes type, Handedness handedness)
		{
#if MAGICLEAP
			if (type == ControllerHelper.ControllerTypes.MagicLeap) 
			{
				InteractorBehaviorControls.Instance.GetInteractorForController(handedness, out _controllerInteractor);
			}
#endif
		}

		private void OnControllerLost(ControllerHelper.ControllerTypes type, Handedness handedness)
		{
#if MAGICLEAP
			if (type == ControllerHelper.ControllerTypes.MagicLeap)
			{
				_controllerInteractor = null;
			}
#endif
		}

		private void OnDisable()
		{
			if (_pointerClickHandler != null)
			{
				_pointerClickHandler.PointerUp -= PointerClicked;
			}

			if (_controllerHelper != null)
			{
				_controllerHelper.controllerSourceDetected.RemoveListener(OnControllerDetected);
				_controllerHelper.controllerSourceLost.RemoveListener(OnControllerLost);
#if MAGICLEAP
				_controllerHelper.controllerTriggerClicked.RemoveListener(OnTriggerClicked);
#endif
			}
		}
	}
}