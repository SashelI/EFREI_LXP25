using System;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.MRTK_Enhanced
{
	public class InteractorsHandler : MonoBehaviour
	{
		public XRInteractionManager interactionManager;

		public List<XRBaseInteractor> ActiveInteractors { get; protected set; } = new();

		public event Action<InteractorRegisteredEventArgs> InteractorRegistered;

		public event Action<InteractorUnregisteredEventArgs> InteractorUnregistered;

		public event Action<SelectEnterEventArgs> SelectEntered;

		public event Action<SelectExitEventArgs> SelectExited;

		public event Action<HoverEnterEventArgs> HoverEntered;

		public event Action<HoverExitEventArgs> HoverExited;

		private static InteractorsHandler _instance;

		public static InteractorsHandler Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<InteractorsHandler>();

					if (_instance == null)
					{
						_instance = GameObject.Find("Astrolabe").AddComponent<InteractorsHandler>();
					}
				}

				return _instance;
			}
		}

		private void OnEnable()
		{
			if (interactionManager != null)
			{
				interactionManager.interactorRegistered += OnInteractorRegistered;
				interactionManager.interactorUnregistered += OnInteractorUnregistered;
			}

			AddAllListeners();
		}

		private void OnDisable()
		{
			if (interactionManager != null)
			{
				interactionManager.interactorRegistered -= OnInteractorRegistered;
				interactionManager.interactorUnregistered -= OnInteractorUnregistered;
			}

			RemoveAllListeners();
		}

		private void OnInteractorRegistered(InteractorRegisteredEventArgs args) //TODO controller magicleap ?
		{
			if (args.interactorObject is XRBaseInteractor interactor)
			{
				interactor.selectEntered.AddListener(OnSelectEntered);
				interactor.selectExited.AddListener(OnSelectExited);

				interactor.hoverEntered.AddListener(OnHoverEntered);
				interactor.hoverExited.AddListener(OnHoverExited);

				ActiveInteractors.Add(interactor);
			}

			InteractorRegistered?.Invoke(args);
		}

		private void OnInteractorUnregistered(InteractorUnregisteredEventArgs args)
		{
			if (args.interactorObject is XRBaseInteractor interactor)
			{
				interactor.selectEntered.RemoveListener(OnSelectEntered);
				interactor.selectExited.RemoveListener(OnSelectExited);

				interactor.hoverEntered.RemoveListener(OnHoverEntered);
				interactor.hoverExited.RemoveListener(OnHoverExited);

				ActiveInteractors.Remove(interactor);
			}

			InteractorUnregistered?.Invoke(args);
		}

		private void OnSelectExited(SelectExitEventArgs eventArgs)
		{
			SelectExited?.Invoke(eventArgs);
		}

		private void OnSelectEntered(SelectEnterEventArgs eventArgs)
		{
			SelectEntered?.Invoke(eventArgs);
		}

		private void OnHoverExited(HoverExitEventArgs eventArgs)
		{
			HoverExited?.Invoke(eventArgs);
		}

		private void OnHoverEntered(HoverEnterEventArgs eventArgs)
		{
			HoverEntered?.Invoke(eventArgs);
		}

		private void RemoveAllListeners()
		{
			foreach (var interactor in ActiveInteractors)
			{
				interactor.selectEntered.RemoveListener(OnSelectEntered);
				interactor.selectExited.RemoveListener(OnSelectExited);

				interactor.hoverEntered.RemoveListener(OnHoverEntered);
				interactor.hoverExited.RemoveListener(OnHoverExited);
			}
		}

		private void AddAllListeners()
		{
			foreach (var interactor in ActiveInteractors)
			{
				interactor.selectEntered.AddListener(OnSelectEntered);
				interactor.selectExited.AddListener(OnSelectExited);

				interactor.hoverEntered.AddListener(OnHoverEntered);
				interactor.hoverExited.AddListener(OnHoverExited);
			}
		}

		public Handedness GetInteractorHandedness(XRBaseInteractor interactor)
		{
			if (interactor is IHandedInteractor handedInteractor)
			{
				return handedInteractor.Handedness;
			}
			else
			{
				if (interactor.transform.parent.name.Contains("left"))
				{
					return Handedness.Left;
				}
				else if (interactor.transform.parent.name.Contains("right"))
				{
					return Handedness.Right;
				}
			}

			return Handedness.None;
		}

		public void GetInteractorsFromHand(ActionBasedController hand, out XRRayInteractor rayInteractor, out PokeInteractor pokeInteractor)
		{
			rayInteractor = null;
			pokeInteractor = null;

			if (hand != null)
			{
				rayInteractor = hand.GetComponentInChildren<XRRayInteractor>();
				pokeInteractor = hand.GetComponentInChildren<PokeInteractor>();
			}
		}
	}
}