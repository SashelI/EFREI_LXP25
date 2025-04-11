using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualManipulator : VisualRectangle, IVisualManipulator
	{
		public new static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalManipulator), typeof(VisualManipulator));
		}

		public VisualManipulator(ILogicalElement logicalElement) : base(logicalElement)
		{
			// On ajoute au rectangle le manipulator
			//this.objectManipulator =  this.rectangle.gameObject.AddComponent<ObjectManipulator>();

			var roundedQuad = rectangle.RoundedQuad.gameObject;
			objectManipulator = roundedQuad.AddComponent<ObjectManipulator>();
			roundedQuad.AddComponent<MRTKBaseInteractable>(); //TODO near

			rectangle.NearInteractionGrabbableCollider = roundedQuad.GetComponent<BoxCollider>();

			objectManipulator.selectEntered.AddListener(OnManipulationStarted);
			objectManipulator.selectExited.AddListener(OnManipulationEnded);
		}

		protected ObjectManipulator objectManipulator;

		//TODO gérer manipulation types
		public ManipulationTypes ManipulationType
		{
			get => _manipulationType;

			set => _manipulationType = value;
		}

		private ManipulationTypes _manipulationType;

		private void OnManipulationStarted(SelectEnterEventArgs eventData)
		{
			PointerRouter.Instance.ExecutePointerEvent(eventData, LogicalElementHandledEvent.PointerPressed);
		}

		private void OnManipulationEnded(SelectExitEventArgs eventData)
		{
			PointerRouter.Instance.ExecutePointerEvent(eventData, LogicalElementHandledEvent.PointerReleased);
		}

		public void SetTarget(LogicalElement target)
		{
			Transform transform = null;

			if (target is LogicalWindow == false)
			{
				// si on ne vise pas une fenetre on va viser le RenderTransform car il n'est pas soumis au Layout (en cas de modification du Layout, Manipulator conservera sa position)
				((VisualElement)target.VisualElement).EnsureRenderTransformExists();

				// fait pointer vers le RenderTransform et donc accessible via Translation
				transform = target.GetGameObject().transform.GetChild(0).transform;

				// normalement dans ce mode la rotation ne devrait pas être autorisé car la rotation doit s'effectuer sur un autre transform
			}
			else
			{
				// C'est un fenetre on passe en Absolute
				transform = target.GetGameObject().transform;
			}

			objectManipulator.HostTransform = transform;
		}

		public void AddConstraint(IVisualConstraint constraint)
		{
			constraint.OnAdd(LogicalElement);
		}

		public void RemoveConstraint(IVisualConstraint constraint)
		{
			constraint.OnRemove();
		}
	}
}