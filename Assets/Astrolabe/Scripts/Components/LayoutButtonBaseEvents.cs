using System;
using System.Collections.Generic;
using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.Components
{
	/// <summary>
	/// LayoutButtonBase est au dessus des evenements qui s'execute dans ButtonContainer (son enfant)
	/// On sépare donc en deux. LayoutButtonBaseEvents gère les events et est placé dans ButtonContainer
	/// </summary>
	public class LayoutButtonBaseEvents : MonoBehaviour, IXRSelectInteractable, IXRFocusInteractable, IXRHoverInteractable
	{
		private bool _isTouchPressed = false;
		private BaseInteractionEventArgs _handTrackingInputEventData;

		public void OnFocusEntered(FocusEnterEventArgs eventData)
		{
			PointerRouter.Instance.ExecuteFocusEnterEvent(eventData);
		}

		public void OnFocusExited(FocusExitEventArgs eventData)
		{
			PointerRouter.Instance.ExecuteFocusExitEvent(eventData);
		}

		public VisualElement VisualElement { get; internal set; }

		public void OnSelectEntered(SelectEnterEventArgs eventData)
		{
			_isTouchPressed = PointerRouter.Instance.ExecutePointerEvent(eventData, LogicalElementHandledEvent.PointerPressed, VisualElement.LogicalElement);

			if (_isTouchPressed)
			{
				_handTrackingInputEventData = eventData;
			}
			else
			{
				_handTrackingInputEventData = null;
			}
		}

		//Warning : OnSelectExited triggers before OnClick

		public void OnSelectExited(SelectExitEventArgs eventData)
		{
			if (_isTouchPressed == true)
			{
				PointerRouter.Instance.ExecutePointerEvent(eventData, LogicalElementHandledEvent.PointerReleased, VisualElement.LogicalElement);
			}
		}

		public void OnClick()
		{
			if (_handTrackingInputEventData != null)
			{
				PointerRouter.Instance.ExecutePointerEvent(_handTrackingInputEventData, LogicalElementHandledEvent.PointerTapped, VisualElement.LogicalElement);

				_handTrackingInputEventData = null;
				_isTouchPressed = false;
			}
		}


		public event Action<InteractableRegisteredEventArgs> registered;
		public event Action<InteractableUnregisteredEventArgs> unregistered;
		public InteractionLayerMask interactionLayers { get; }
		public List<Collider> colliders { get; }

		public Transform GetAttachTransform(IXRInteractor interactor)
		{
			return null;
		}

		public void OnRegistered(InteractableRegisteredEventArgs args)
		{
		}

		public void OnUnregistered(InteractableUnregisteredEventArgs args)
		{
		}

		public void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
		{
		}

		public float GetDistanceSqrToInteractor(IXRInteractor interactor)
		{
			return 0;
		}

		public SelectEnterEvent firstSelectEntered { get; } = new ();
		public SelectExitEvent lastSelectExited { get; } = new();
		public SelectEnterEvent selectEntered { get; } = new();
		public SelectExitEvent selectExited { get; } = new();
		public List<IXRSelectInteractor> interactorsSelecting { get; } = new();
		public IXRSelectInteractor firstInteractorSelecting { get; }
		public bool isSelected { get; }
		public InteractableSelectMode selectMode { get; }

		public bool IsSelectableBy(IXRSelectInteractor interactor)
		{
			return true;
		}

		public Pose GetAttachPoseOnSelect(IXRSelectInteractor interactor)
		{
			return new Pose();
		}

		public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractor interactor)
		{
			return new Pose();
		}

		public void OnSelectEntering(SelectEnterEventArgs args)
		{
		}

		public void OnSelectExiting(SelectExitEventArgs args)
		{
		}

		public FocusEnterEvent firstFocusEntered { get; } = new();
		public FocusExitEvent lastFocusExited { get; } = new();
		public FocusEnterEvent focusEntered { get; } = new();
		public FocusExitEvent focusExited { get; } = new();
		public List<IXRInteractionGroup> interactionGroupsFocusing { get; } = new();
		public IXRInteractionGroup firstInteractionGroupFocusing { get; }
		public bool isFocused { get; }
		public InteractableFocusMode focusMode { get; }
		public bool canFocus { get; }

		public void OnFocusEntering(FocusEnterEventArgs args)
		{
		}

		public void OnFocusExiting(FocusExitEventArgs args)
		{
		}

		public bool IsHoverableBy(IXRHoverInteractor interactor)
		{
			return true;
		}

		public void OnHoverEntering(HoverEnterEventArgs args)
		{
		}

		public void OnHoverEntered(HoverEnterEventArgs args)
		{
			PointerRouter.Instance.ExecuteHoverEnterEvent(args);
		}

		public void OnHoverExiting(HoverExitEventArgs args)
		{
		}

		public void OnHoverExited(HoverExitEventArgs args)
		{
			PointerRouter.Instance.ExecuteHoverExitEvent(args);
		}

		public HoverEnterEvent firstHoverEntered { get; } = new();
		public HoverExitEvent lastHoverExited { get; } = new();
		public HoverEnterEvent hoverEntered { get; } = new();
		public HoverExitEvent hoverExited { get; } = new();
		public List<IXRHoverInteractor> interactorsHovering { get; } = new();
		public bool isHovered { get; }
	}
}