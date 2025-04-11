using System;
using System.Collections.Generic;
using Assets.Astrolabe.Scripts.Tools;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	public class PointerDraggedEventArgs
	{
		public XRBaseInteractor Pointer;
		public Vector3 StartPosition;
		public Vector3 EndPosition;
		public Vector3 DeltaPosition;
		public Handedness Handedness;

		public PointerDraggedEventArgs(XRBaseInteractor pointer, Vector3 startPosition, Vector3 endPosition, Vector3 deltaPosition, Handedness handedness)
		{
			Pointer = pointer;
			StartPosition = startPosition;
			EndPosition = endPosition;
			DeltaPosition = deltaPosition;
			Handedness = handedness;
		}
	}

	public class PointerDragHandler : MonoBehaviour
	{
		public float dragThreshold = 0.005f;

		private HashSet<Handedness> _trackedControllers = new();

		/// <summary>
		/// _dico[interactor] = Tuple(position, isHand)
		/// </summary>
		private readonly Dictionary<XRBaseInputInteractor, Tuple<Vector3, bool>> _activeInteractors = new();
		private readonly Dictionary<PokeInteractor, Tuple<Vector3, bool>> _activePokeInteractors = new();

		public event Action<PointerDraggedEventArgs> PointerDragged;
		public event Action<PointerDraggedEventArgs> PokePointerDragged;
		public event Action<PointerClickedEventArgs> DragSelectExited;

		private InteractorsHandler _interactorsHandler;

		private static PointerDragHandler _instance;

		public static PointerDragHandler Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<PointerDragHandler>();

					if (_instance == null)
					{
						_instance = GameObject.Find("Astrolabe").AddComponent<PointerDragHandler>();
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

			var pointerclickHandler = PointerClickHandler.Instance;
			if (pointerclickHandler != null)
			{
				pointerclickHandler.PointerDown += OnPointerClicked;
				pointerclickHandler.PointerUp += OnPointerStopClick;
			}
		}

		private void OnDisable()
		{
			var pointerclickHandler = PointerClickHandler.Instance;
			if (pointerclickHandler != null)
			{
				pointerclickHandler.PointerDown -= OnPointerClicked;
				pointerclickHandler.PointerUp -= OnPointerStopClick;
			}
		}

		private void OnPointerStopClick(PointerClickedEventArgs args)
		{
			if (args.Pointer == null)
			{
				var hand = args.Handedness switch
				{
					Handedness.Left => PointerClickHandler.Instance?.LeftHand,
					Handedness.Right => PointerClickHandler.Instance?.RightHand,
					Handedness.None => PointerClickHandler.Instance?.MLController,
					_ => null
				};

				if (hand != null)
				{
					_interactorsHandler.GetInteractorsFromHand(hand, out var rayInteractor, out var pokeInteractor);
					if (rayInteractor != null)
					{
						_activeInteractors.Remove(rayInteractor);
					}
					else if (pokeInteractor != null)
					{
						_activePokeInteractors.Remove(pokeInteractor);
					}

				}
			}
			else
			{
				var interactor = args.Pointer;

				if (interactor is XRRayInteractor rayInteractor && _activeInteractors.ContainsKey(rayInteractor))
				{
					_activeInteractors.Remove(rayInteractor);
				}
				else if (interactor is PokeInteractor pokeInteractor && _activePokeInteractors.ContainsKey(pokeInteractor))
				{
					_activePokeInteractors.Remove(pokeInteractor);
				}
			}
		}

		private void OnPointerClicked(PointerClickedEventArgs args)
		{
			if (args.Pointer == null)
			{
				var hand = args.Handedness switch
				{
					Handedness.Left => PointerClickHandler.Instance?.LeftHand,
					Handedness.Right => PointerClickHandler.Instance?.RightHand,
					Handedness.None => PointerClickHandler.Instance?.MLController,
					_ => null
				};

				if (hand != null)
				{
					_interactorsHandler.GetInteractorsFromHand(hand, out var rayInteractor, out var pokeInteractor);

					if (rayInteractor != null && !_activeInteractors.ContainsKey(rayInteractor))
					{
						_activeInteractors[rayInteractor] = new(rayInteractor.transform.position, HandJointUtils.FindHandFromScene(args.Handedness));
					}
					else if (pokeInteractor != null && !_activePokeInteractors.ContainsKey(pokeInteractor))
					{
  						_activePokeInteractors[pokeInteractor] = new(pokeInteractor.transform.position, HandJointUtils.FindHandFromScene(args.Handedness));
					}

				}
			}
			else
			{
				var interactor = args.Pointer;

				if (interactor is XRRayInteractor rayInteractor && !_activeInteractors.ContainsKey(rayInteractor))
				{
					_activeInteractors[rayInteractor] = new(rayInteractor.transform.position, HandJointUtils.FindHandFromScene(args.Handedness));
				}
				else if (interactor is PokeInteractor pokeInteractor && !_activePokeInteractors.ContainsKey(pokeInteractor))
				{
					_activePokeInteractors[pokeInteractor] = new(pokeInteractor.transform.position, HandJointUtils.FindHandFromScene(args.Handedness));
				}
			}
		}

		private void OnSelectEntered(SelectEnterEventArgs args)
		{
			if (args.interactableObject.transform.gameObject.layer == LayerMask.NameToLayer("UI")) { return; } //We don't throw drag events on UI

			if (args.interactorObject is XRBaseInputInteractor interactor)
			{
				Handedness handedness = interactor.handedness == InteractorHandedness.Left
					? Handedness.Left
					: Handedness.Right;

				if (HandJointUtils.TryGetPinch(handedness, out _, out bool isPinching, out _) && isPinching)
				{
					_activeInteractors[interactor] = new(interactor.transform.position, HandJointUtils.FindHandFromScene(handedness));
				}
			}
		}

		private void OnSelectExited(SelectExitEventArgs args)
		{
			if (args.interactableObject.transform.gameObject.layer == LayerMask.NameToLayer("UI")) { return; }

			if (args.interactorObject is XRBaseInputInteractor interactor && _activeInteractors.ContainsKey(interactor))
			{
				_activeInteractors.Remove(interactor);

				DragSelectExited?.Invoke(
					new PointerClickedEventArgs(interactor, interactor.transform.position, args.interactableObject.transform,
					interactor.transform.position, _interactorsHandler.GetInteractorHandedness(interactor)));
			}
		}

		private void FixedUpdate()
		{
			//Far pointers
			foreach (var kvp in new Dictionary<XRBaseInputInteractor, Tuple<Vector3, bool>>(_activeInteractors))
			{
				XRBaseInputInteractor interactor = kvp.Key;
				Vector3 lastPosition = kvp.Value.Item1;
				bool isHand = kvp.Value.Item2;

				Vector3 currentPosition = interactor.transform.position;
				Vector3 delta = currentPosition - lastPosition;

				var handedness = _interactorsHandler.GetInteractorHandedness(interactor);

#if METAQUEST
				//As quest controllers are considered like hands by aggregator (??), need to check here if any controller is tracked
				//Otherwise lasers and explosion will look at finger pinch even with quest controllers (user needs to firmly grasp the controller)
				isHand = _trackedControllers.Contains(handedness) == false;
#endif

				if (delta.magnitude > dragThreshold)
				{
					bool pinching = true;

					if (isHand) //If we are using hands
					{
						HandJointUtils.TryGetPinch(handedness, out _, out pinching, out _);
					}

					if (pinching) //need to check the pinch to avoid odd stuck behavior
					{
						PointerDragged?.Invoke(new PointerDraggedEventArgs(interactor, lastPosition, currentPosition, delta, handedness));
						_activeInteractors[interactor] = new(currentPosition, isHand);
					}
					else
					{
						_activeInteractors.Remove(interactor);
					}
				}
			}

			//Poke pointers
  			foreach (var kvp in new Dictionary<PokeInteractor, Tuple<Vector3, bool>>(_activePokeInteractors))
			{
				PokeInteractor interactor = kvp.Key;
				Vector3 lastPosition = kvp.Value.Item1;
				bool isHand = kvp.Value.Item2;

				Vector3 currentPosition = interactor.transform.position;
				Vector3 delta = currentPosition - lastPosition;

				var handedness = _interactorsHandler.GetInteractorHandedness(interactor);

#if METAQUEST
				//As quest controllers are considered like hands by aggregator (??), need to check here if any controller is tracked
				//Otherwise lasers and explosion will look at finger pinch even with quest controllers (user needs to firmly grasp the controller)
				isHand = _trackedControllers.Contains(handedness) == false;
#endif

				if (delta.magnitude > dragThreshold)
				{
					bool pinching = true;

					if (isHand) //If we are using hands
					{
						HandJointUtils.TryGetPinch(handedness, out _, out pinching, out _);
					}

					if (pinching) //need to check the pinch to avoid odd stuck behavior
					{
						PokePointerDragged?.Invoke(new PointerDraggedEventArgs(interactor, lastPosition, currentPosition, delta, handedness));
						_activePokeInteractors[interactor] = new(currentPosition, isHand);
					}
					else
					{
						_activePokeInteractors.Remove(interactor);
					}
				}
			}
		}

		public void SetControllerTracked(bool tracked, Handedness handedness)
		{
			if (tracked)
			{
				_trackedControllers.Add(handedness);
			}
			else
			{
				_trackedControllers.Remove(handedness);
			}
		}
	}
}