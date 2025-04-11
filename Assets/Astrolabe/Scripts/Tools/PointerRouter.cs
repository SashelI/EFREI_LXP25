using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.Tools
{
	public class PointerRouter
	{
		public static PointerRouter Instance => _instance;

		private static readonly PointerRouter _instance = new();

		private ILogicalElement GetLogicalElement(GameObject hitObject)
		{
			if (hitObject == null)
			{
				return null;
			}

			var component = hitObject.GetComponentInParent<TwinkleComponent>();

			return component?.VisualElement?.LogicalElement;
		}

		// on evite les allocations au maximum
		private readonly PointerFocusEventArgs _focusArgs = new(null, null);

		private void ExecuteFocusEvent(ILogicalElement newFocusElement, ILogicalElement oldFocusElement,
			LogicalElementHandledEvent pointerEvent, PointerTypes pointerType)
		{
			_focusArgs.OldValue = oldFocusElement;
			_focusArgs.NewValue = newFocusElement;

			ILogicalElement logicalElement = null;

			switch (pointerEvent)
			{
				case LogicalElementHandledEvent.PointerEntered:
					logicalElement = newFocusElement;
					break;

				case LogicalElementHandledEvent.PointerExited:
					logicalElement = oldFocusElement;
					break;
			}

			if (logicalElement != null)
			{
				logicalElement.ExecuteExternalEvent(pointerType, pointerEvent, _focusArgs);
			}
		}

		private void ExecuteHoverEvent(ILogicalElement hoveredElement, LogicalElementHandledEvent pointerEvent,
			PointerTypes pointerType)
		{
			switch (pointerEvent)
			{
				case LogicalElementHandledEvent.PointerEntered:
					_focusArgs.OldValue = null;
					_focusArgs.NewValue = hoveredElement;
					break;

				case LogicalElementHandledEvent.PointerExited:
					_focusArgs.OldValue = hoveredElement;
					_focusArgs.NewValue = null;
					break;
			}

			if (hoveredElement != null)
			{
				hoveredElement.ExecuteExternalEvent(pointerType, pointerEvent, _focusArgs);
			}
		}

		public void ExecuteHoverEnterEvent(HoverEnterEventArgs eventData)
		{
			ILogicalElement oldFocusElement = null;
			ILogicalElement newFocusElement = null;

			var lastHover = eventData.manager.lastFocused;
			if (lastHover != null)
			{
				oldFocusElement = GetLogicalElement(lastHover.transform.gameObject);
			}

			var currentHover = eventData.interactableObject;
			if (currentHover != null)
			{
				newFocusElement = GetLogicalElement(currentHover.transform.gameObject);
			}

			var pointerType = GetPointerType(eventData.interactorObject);

			ExecuteFocusEvent(newFocusElement, oldFocusElement, LogicalElementHandledEvent.PointerEntered, pointerType);
		}

		public void ExecuteHoverExitEvent(HoverExitEventArgs eventData)
		{
			var hoveredElement = GetLogicalElement(eventData.interactableObject.transform.gameObject);

			var pointerType = GetPointerType(eventData.interactorObject);

			ExecuteHoverEvent(hoveredElement, LogicalElementHandledEvent.PointerExited, pointerType);
		}

		public void ExecuteFocusEnterEvent(FocusEnterEventArgs eventData)
		{
			var oldFocusElement = GetLogicalElement(eventData.manager.lastFocused.transform.gameObject);
			var newFocusElement = GetLogicalElement(eventData.interactableObject.transform.gameObject);

			var pointerType = GetPointerType(eventData.interactorObject);

			ExecuteFocusEvent(newFocusElement, oldFocusElement, LogicalElementHandledEvent.PointerEntered, pointerType);
		}

		public void ExecuteFocusExitEvent(FocusExitEventArgs eventData)
		{
			var oldFocusElement = GetLogicalElement(eventData.manager.lastFocused.transform.gameObject);
			var newFocusElement = GetLogicalElement(eventData.interactableObject.transform.gameObject);

			var pointerType = GetPointerType(eventData.interactorObject);

			ExecuteFocusEvent(newFocusElement, oldFocusElement, LogicalElementHandledEvent.PointerExited, pointerType);
		}

		private readonly PointerEventArgs _pointerArgs = new();

		public void ExecutePointerEvent(BaseInteractionEventArgs eventData,
			LogicalElementHandledEvent logicalElementRoutedEvent)
		{
			ILogicalElement logicalElement = null;

			try
			{
				var hitObject = eventData.interactableObject.transform.gameObject;
				logicalElement = GetLogicalElement(hitObject);

				// en cas de null il faudra sans doute renvoyer à App
				if (logicalElement != null)
				{
					var pointerType = GetPointerType(eventData.interactorObject);

					if (eventData.interactorObject is XRRayInteractor rayInteractor)
					{
						_pointerArgs.StartRay = rayInteractor.rayOriginTransform.position.ToVector3();
						_pointerArgs.EndRay = rayInteractor.rayEndPoint.ToVector3();
						_pointerArgs.Point =
							hitObject.transform.InverseTransformPoint(rayInteractor.rayEndPoint).ToVector3();
					}
					else if (eventData.interactorObject is XRDirectInteractor directInteractor)
					{
						_pointerArgs.StartRay = directInteractor.transform.position.ToVector3();
						_pointerArgs.EndRay = directInteractor.transform.position.ToVector3();
						_pointerArgs.Point = hitObject.transform.InverseTransformPoint(directInteractor.transform.position)
							.ToVector3();
					}

					logicalElement.ExecuteExternalEvent(pointerType, logicalElementRoutedEvent, _pointerArgs);
				}
			}
			catch
			{
			}
		}

		private PointerTypes GetPointerType(IXRInteractor interactor)
		{
			if (interactor == null)
			{
				return PointerTypes.Unknown;
			}

			var handedness = Handedness.None;

			if (interactor is IHandedInteractor iHandInteractor)
			{
				handedness = iHandInteractor.Handedness;
			}
			else
			{
				if (interactor.transform.parent.name.Contains("left"))
				{
					handedness = Handedness.Left;
				}
				else if (interactor.transform.parent.name.Contains("right"))
				{
					handedness = Handedness.Right;
				}
			}

			switch (interactor)
			{
				case XRRayInteractor:
					return handedness switch
					{
						Handedness.Right => PointerTypes.RightFarPointer,
						Handedness.Left => PointerTypes.LeftFarPointer,
						_ => PointerTypes.Unknown
					};
					break;

				case XRDirectInteractor:
					return handedness switch
					{
						Handedness.Right => PointerTypes.RightNearPointer,
						Handedness.Left => PointerTypes.LeftNearPointer,
						_ => PointerTypes.Unknown
					};
					break;

				case PokeInteractor:
					return handedness switch
					{
						Handedness.Right => PointerTypes.RightNearPointer,
						Handedness.Left => PointerTypes.LeftNearPointer,
						_ => PointerTypes.Unknown
					};
					break;

				default:
					return PointerTypes.Unknown;
			}
		}

		public bool ExecutePointerEvent(BaseInteractionEventArgs eventData,
			LogicalElementHandledEvent logicalElementRoutedEvent, ILogicalElement logicalElement = null)
		{
			GameObject hitObject = null;
			ILogicalElement logicalElementHitted = null;

			if (logicalElement == null)
			{
				hitObject = eventData.interactableObject.transform.gameObject;
				logicalElementHitted = GetLogicalElement(hitObject);
			}
			else
			{
				logicalElementHitted = logicalElement;
			}

			// en cas de null il faudra sans doute renvoyer à App
			if (logicalElementHitted != null)
			{
				_pointerArgs.StartRay = eventData.interactorObject.transform.position.ToVector3();

				if (eventData.interactorObject is XRDirectInteractor directInteractor && hitObject != null)
				{
					_pointerArgs.EndRay = directInteractor.transform.position.ToVector3();
					_pointerArgs.Point = hitObject.transform.InverseTransformPoint(directInteractor.transform.position)
						.ToVector3();
				}
				else if (eventData.interactorObject is CanvasProxyInteractor unityInteractor && hitObject != null)
				{
					_pointerArgs.EndRay = unityInteractor.transform.position.ToVector3();
					_pointerArgs.Point = hitObject.transform.InverseTransformPoint(unityInteractor.transform.position)
						.ToVector3();
				}
				else
				{
					_pointerArgs.EndRay = global::Astrolabe.Twinkle.Vector3.NaN;
					_pointerArgs.Point = global::Astrolabe.Twinkle.Vector3.NaN;
				}

				var pointerType = GetPointerType(eventData.interactorObject);

				logicalElementHitted.ExecuteExternalEvent(pointerType, logicalElementRoutedEvent, _pointerArgs);

				return true;
			}

			return false;
		}
	}
}