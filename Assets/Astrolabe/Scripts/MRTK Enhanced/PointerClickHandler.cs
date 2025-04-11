using System;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ControllerLookup = Assets.Astrolabe.Scripts.MRTK_Enhanced.AstrolabeControllerLookup;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	public class PointerClickedEventArgs
	{
		public XRBaseInteractor Pointer;
		public Vector3 ClickPosition;
		public Vector3 TargetHitPosition;
		public Handedness Handedness;
		public Transform Target;

		public PointerClickedEventArgs(XRBaseInteractor pointer, Vector3 clickPosition, Transform target, Vector3 targetHitPosition, Handedness handedness)
		{
			Pointer = pointer;
			ClickPosition = clickPosition;
			Handedness = handedness;
			Target = target;
			TargetHitPosition = targetHitPosition;
		}
	}

	/// <summary>
	/// To keep track of click on anything but UI
	/// </summary>
	public class PointerClickHandler : MonoBehaviour
	{
		private PointerClickedEventArgs _lastClickEventArgsLeft;
		private PointerClickedEventArgs _lastClickEventArgsRight;
		private PointerClickedEventArgs _lastClickEventArgsNone;

		public event Action<PointerClickedEventArgs> PointerDown;
		public event Action<PointerClickedEventArgs> PointerUp;

		public ControllerLookup ControllerLookup => _controllerLookup;
		public ArticulatedHandController LeftHand => (ArticulatedHandController)_controllerLookup.LeftHandController;
		public ArticulatedHandController RightHand => (ArticulatedHandController)_controllerLookup.RightHandController;
		public ActionBasedController MLController => (ActionBasedController)_controllerLookup.MLController;

		private InteractorsHandler _interactorsHandler;
		private ControllerLookup _controllerLookup;

		private bool _isClickingTargetBoxLeft = false;
		private bool _isClickingTargetBoxRight = false;
		private bool _isClickingTargetBoxNone = false;

		private bool _isDownLeft = false;
		private bool _isDownRight = false;
		private bool _isDownNone = false;

		private static PointerClickHandler _instance;

		public static PointerClickHandler Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<PointerClickHandler>();

					if (_instance == null)
					{
						_instance = GameObject.Find("Astrolabe").AddComponent<PointerClickHandler>();
					}
				}

				return _instance;
			}
		}

		private void OnEnable()
		{
			if (_interactorsHandler == null)
			{
				_interactorsHandler = InteractorsHandler.Instance;
			}

			if (_interactorsHandler != null)
			{
				_interactorsHandler.SelectEntered += OnSelectEntered;
				_interactorsHandler.SelectExited += OnSelectExited;
			}
		}

		private void OnDisable()
		{
			if (_interactorsHandler != null)
			{
				_interactorsHandler.SelectEntered -= OnSelectEntered;
				_interactorsHandler.SelectExited -= OnSelectExited;

				_isClickingTargetBoxLeft = false;
				_isClickingTargetBoxRight = false;
				_isClickingTargetBoxNone = false;
			}
		}

		private void Start()
		{
			GetHandControllerLookup();
		}

		private void Update()
		{
			// Prevents sending these events when there is an actual target (SelectExit will send it instead)
			// Also, if no one is subscribed to the events, we don't bother processing the data (can be expensive)
			if (PointerDown == null && PointerUp == null) { return; }
			if (!_isClickingTargetBoxLeft)
			{
				if (GetIsTriggered(LeftHand))
				{
					var pointerEventArgs = new PointerClickedEventArgs(null, Vector3.positiveInfinity, null, Vector3.positiveInfinity, Handedness.Left);
					if (IsClickingOnTarget(Handedness.Left, out XRBaseInteractor interactor, out var position, out Transform target, out var hitPosition))
					{
						if (target.gameObject.layer == LayerMask.NameToLayer("UI")) { return; } //Shouldn't throw a click on UI

						pointerEventArgs = new PointerClickedEventArgs(interactor, position, target, hitPosition, Handedness.Left);
					}

					if (!_isDownLeft)
					{
						PointerDown?.Invoke(pointerEventArgs);
						_isDownLeft = true;
					}
					_lastClickEventArgsLeft = pointerEventArgs;
				}
				else if (_isDownLeft)
				{
					PointerUp?.Invoke(_lastClickEventArgsLeft);
					_isDownLeft = false;
				}
			}

			if (!_isClickingTargetBoxRight)
			{
				if (GetIsTriggered(RightHand))
				{
					var pointerEventArgs = new PointerClickedEventArgs(null, Vector3.positiveInfinity, null, Vector3.positiveInfinity, Handedness.Right);
					if (IsClickingOnTarget(Handedness.Right, out XRBaseInteractor interactor, out var position, out Transform target, out var hitPosition))
					{
						if (target.gameObject.layer == LayerMask.NameToLayer("UI")) { return; } //Shouldn't throw a click on UI

						pointerEventArgs = new PointerClickedEventArgs(interactor, position, target, hitPosition, Handedness.Right);
					}

					if (!_isDownRight)
					{
						PointerDown?.Invoke(pointerEventArgs);
						_isDownRight = true;
					}
					_lastClickEventArgsRight = pointerEventArgs;
				}
				else if (_isDownRight)
				{
					PointerUp?.Invoke(_lastClickEventArgsRight);
					_isDownRight = false;
				}
			}

#if MAGICLEAP
			if (!_isClickingTargetBoxNone)
			{
				if (GetIsTriggered(MLController))
				{
					var pointerEventArgs = new PointerClickedEventArgs(null, Vector3.positiveInfinity, null, Vector3.positiveInfinity, Handedness.None);
					if (IsClickingOnTarget(Handedness.None, out XRBaseInteractor interactor, out var position, out Transform target, out var hitPosition))
					{
						if (target.gameObject.layer == LayerMask.NameToLayer("UI")) { return; } //Shouldn't throw a click on UI

						pointerEventArgs = new PointerClickedEventArgs(interactor, position, target, hitPosition, Handedness.None);
					}
					
					if(!_isDownNone)
					{
						PointerDown?.Invoke(pointerEventArgs);
						_isDownNone = true;
					}
					_lastClickEventArgsNone = pointerEventArgs;
				}
				else if (_isDownNone)
				{
					PointerUp?.Invoke(_lastClickEventArgsNone);
					_isDownNone = false;
				}
			}	
#endif
		}

		private void GetHandControllerLookup()
		{
			if (_controllerLookup != null) { return; }

			ControllerLookup[] lookups = FindObjectsOfType(typeof(ControllerLookup)) as ControllerLookup[];

			if (lookups.Length == 0)
			{
				Debug.LogError("Could not locate an instance of the ControllerLookup ...");
			}
			else if (lookups.Length > 1)
			{
				Debug.LogWarning("Found more than one instance of the ControllerLookup ....");

				_controllerLookup = lookups[0];
			}
			else
			{
				_controllerLookup = lookups[0];
			}
		}

		public bool GetIsTriggered(ArticulatedHandController hand)
		{
			return hand.currentControllerState.selectInteractionState.value > 0.95f;
		}

		public bool GetIsTriggered(ActionBasedController controller)
		{
			return controller.currentControllerState.selectInteractionState.value > 0.95f;
		}

		private void OnSelectEntered(SelectEnterEventArgs args)
		{
			if (args.interactableObject.transform.gameObject.layer == LayerMask.NameToLayer("UI")) { return; }
			if (args.interactorObject is XRBaseInteractor interactor)
			{
				Vector3 position = interactor.transform.position;
				Handedness handedness = _interactorsHandler.GetInteractorHandedness(interactor);
				switch (handedness)
				{
					case Handedness.Left:
						_isClickingTargetBoxLeft = true;
						break;
					case Handedness.Right:
						_isClickingTargetBoxRight = true;
						break;
					case Handedness.None:
						_isClickingTargetBoxNone = true;
						break;
					default:
						_isClickingTargetBoxLeft = true;
						_isClickingTargetBoxRight = true;
						break;
				}
				TryGetTarget(interactor, out var target, out var hitPosition);
				PointerDown?.Invoke(new PointerClickedEventArgs(interactor, position, target, hitPosition, handedness));
			}
		}

		private void OnSelectExited(SelectExitEventArgs args)
		{
			//If no one is subscribed to the event, we don't bother processing the data (can be expensive)
			if (PointerUp == null)
			{
				_isClickingTargetBoxLeft = false;
				_isClickingTargetBoxRight = false;
				_isClickingTargetBoxNone = false;
				return;
			}

			if (args.interactableObject.transform.gameObject.layer == LayerMask.NameToLayer("UI")) { return; }

			if (args.interactorObject is XRBaseInteractor interactor)
			{
				Vector3 position = interactor.transform.position;
				Handedness handedness = _interactorsHandler.GetInteractorHandedness(interactor);
				PointerUp?.Invoke(new PointerClickedEventArgs(interactor, position, args.interactableObject.transform, args.interactableObject.transform.position, handedness));
				switch (handedness)
				{
					case Handedness.Left:
						_isClickingTargetBoxLeft = false;
						break;
					case Handedness.Right:
						_isClickingTargetBoxRight = false;
						break;
					case Handedness.None:
						_isClickingTargetBoxNone = false;
						break;
					default:
						_isClickingTargetBoxLeft = false;
						_isClickingTargetBoxRight = false;
						break;
				}
			}
			else
			{
				_isClickingTargetBoxLeft = false;
				_isClickingTargetBoxRight = false;
				_isClickingTargetBoxNone = false;
			}
		}

		/// <summary>
		/// To check if this is a void click or a click on a non-interactable collider
		/// </summary>
		/// <param name="handedness"></param>
		/// <param name="interactor"></param>
		/// <param name="clickPosition"></param>
		/// <param name="target"></param>
		/// <param name="hitPosition"></param>
		/// <returns></returns>
		private bool IsClickingOnTarget(Handedness handedness, out XRBaseInteractor interactor, out Vector3 clickPosition, out Transform target, out Vector3 hitPosition)
		{
			clickPosition = Vector3.positiveInfinity;
			interactor = null;
			target = null;
			hitPosition = Vector3.positiveInfinity;

			var hand = handedness switch
			{
				Handedness.Left => LeftHand,
				Handedness.Right => RightHand,
				Handedness.None => MLController,
				_ => null
			};

			if (hand != null)
			{
				var rayInteractor = hand.GetComponentInChildren<XRRayInteractor>();
				if (rayInteractor != null && hand.currentControllerState.isTracked)
				{
					interactor = rayInteractor;
					clickPosition = rayInteractor.transform.position;
					if (TryGetTarget(rayInteractor, out var targetTransform, out var position))
					{
						target = targetTransform;
						hitPosition = position;
						return true;
					}
				}

				var nearInteractor = hand.GetComponentInChildren<PokeInteractor>();
				if (nearInteractor != null && hand.currentControllerState.isTracked)
				{
					interactor = nearInteractor;
					clickPosition = nearInteractor.transform.position;
					if (TryGetTarget(nearInteractor, out var targetTransform, out var position))
					{
						target = targetTransform;
						hitPosition = position;
						return true;
					}
				}
			}

			return false;
		}

		private bool TryGetTarget(XRBaseInteractor interactor, out Transform target, out Vector3 hitPosition)
		{
			target = null;
			hitPosition = Vector3.positiveInfinity;

			if (interactor is XRRayInteractor rayInteractor) //Far interaction
			{
				rayInteractor.TryGetCurrent3DRaycastHit(out var hit);
				target = hit.transform;

				if (target != null)
				{
					hitPosition = hit.point;
					return true;
				}
			}
			else if (interactor is PokeInteractor pokeInteractor) //Near interaction
			{
				target = pokeInteractor.firstInteractableSelected?.transform;
				if (target != null)
				{
					hitPosition = pokeInteractor.PokeTrajectory.End;
					return true;
				}
			}

			return false;
		}
	}
}