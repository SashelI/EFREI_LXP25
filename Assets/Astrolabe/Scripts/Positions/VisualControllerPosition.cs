using System;
using System.Linq;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Assets.Astrolabe.Scripts.Positions
{
	/// <summary>
	/// Manages window position on a Controller
	/// </summary>
	public class VisualControllerPosition : VisualSolver<HandConstraint>, IVisualControllerPosition
	{
		private readonly LogicalControllerPosition _logicalController;

		public VisualControllerPosition(LogicalControllerPosition logicalController) : base(logicalController)
		{
			_logicalController = logicalController;
		}

		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalControllerPosition), typeof(VisualControllerPosition));
		}

		public event EventHandler ControllerActivated;
		public event EventHandler ControllerDeactivated;

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);

			solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
			SetInteractorsForSolver();
			solverHandler.TrackedHandedness = Handedness.Both;
			solverHandler.AdditionalOffset = AdditionalOffset;

			behaviour.UpdateWhenOppositeHandNear = true;

			ControllerHand = _controllerHand;
			Placement = _placement;
			PlacementOffset = _placementOffset;

			RotationBehavior = _rotationBehavior;
			OffsetBehavior = _offsetBehavior;

			IsSmoothed = _isSmoothed;

			window.VisualElement.RenderTransformVisibility = Visibility.Collapsed;
		}

		private void SetInteractorsForSolver()
		{
			if (solverHandler != null)
			{
				var rays = GameObject.FindObjectsOfType<XRRayInteractor>();

#if METAQUEST
				solverHandler.RightInteractor = rays.First(ray => ray.transform.parent.name.Contains("Right"));
				solverHandler.LeftInteractor = rays.First(ray => ray.transform.parent.name.Contains("Left"));
#elif MAGICLEAP
				if (ControllerHand == Hands.Left)
				{
					solverHandler.RightInteractor = rays.First(ray => ray.transform.parent.name.Contains("Right"));
					solverHandler.LeftInteractor = rays.First(ray => ray.transform.parent.name.Contains("MagicLeap"));
				}
				else if (ControllerHand == Hands.Right)
				{
					solverHandler.LeftInteractor = rays.First(ray => ray.transform.parent.name.Contains("Left"));
					solverHandler.RightInteractor = rays.First(ray => ray.transform.parent.name.Contains("MagicLeap"));
				}
				else
				{
					solverHandler.LeftInteractor = rays.First(ray => ray.transform.parent.name.Contains("Magicleap"));
					solverHandler.RightInteractor = rays.First(ray => ray.transform.parent.name.Contains("MagicLeap"));
				}
#endif
			}
		}

		public override void OnRemove()
		{
			// On remet la valeur par défaut
			solverHandler.TrackedTargetType = TrackedObjectType.Head;

			window.VisualElement.RenderTransformVisibility = Visibility.Visible;

			base.OnRemove();
		}

		public override bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				var coordinate = Coordinate;

				base.IsEnabled = value;

				if (value == false)
				{
					//On replace les coordonnées de la main car sinon il repasse à zero automatiquement dès qu'ils sont disable
					Coordinate = coordinate;
				}
				else
				{
					if (solverHandler.AdditionalOffset != AdditionalOffset) //Si l'on vient d'une main et que le solver n'a pas été nettoyé, on remet les valeurs
					{
						solverHandler.AdditionalOffset = AdditionalOffset;
					}
				}
			}
		}

		public void OnButtonPressed(ControllerButtons button)
		{
			Debug.Log($"HANDMENU BUTTON CONTROLLER {button} , {TrackedButton} , {IsControllerActivated}");

			if (button == TrackedButton)
			{
				if (solverHandler.LeftInteractor == null || solverHandler.RightInteractor == null)
				{
					SetInteractorsForSolver();
				}

				if (IsControllerActivated)
				{
					OnControllerDeactivate();
				}
				else
				{
					OnControllerActivate();
				}
			}
		}

		private void OnControllerActivate()
		{
			Debug.Log($"HANDMENU BUTTON ACTIVATE {solverHandler.enabled} {solverHandler.LeftInteractor} {solverHandler.CurrentTrackedHandedness}");
			window.VisualElement.RenderTransformVisibility = Visibility.Visible;

			IsControllerActivated = true;
			ControllerActivated?.Invoke(_logicalController, EventArgs.Empty);
		}

		private void OnControllerDeactivate()
		{
			Debug.Log($"HANDMENU BUTTON DEACTIVATE {IsControllerActivated}");
			window.VisualElement.RenderTransformVisibility = Visibility.Collapsed;

			IsControllerActivated = false;
			ControllerDeactivated?.Invoke(_logicalController, EventArgs.Empty);
		}

		/// <summary>
		/// True when window is visible
		/// </summary>
		public bool IsControllerActivated { get; private set; }

		public Hands ControllerHand
		{
			get => _controllerHand;

			set
			{
				_controllerHand = value;

				if (solverHandler != null)
				{
					solverHandler.TrackedHandedness = (Handedness)(int)value;
				}
			}
		}

		private Hands _controllerHand = Hands.None;


		public ControllerButtons TrackedButton
		{
			get => _trackedButton;
			set => _trackedButton = value;
		}

		private ControllerButtons _trackedButton = ControllerButtons.None;

		public ControllerPlacements Placement
		{
			get => _placement;

			set
			{
				_placement = value;

				if (behaviour != null)
				{
					behaviour.SafeZone = (HandConstraint.SolverSafeZone)(int)value;
				}
			}
		}

		private ControllerPlacements _placement = ControllerPlacements.Right;

		public float PlacementOffset
		{
			get => _placementOffset;

			set => _placementOffset = value;
		}

		private float _placementOffset = 0.15f;

		public UnityEngine.Vector3 AdditionalOffset
		{
			get => _additionalOffset;
			set => _additionalOffset = value;
		}

		private UnityEngine.Vector3 _additionalOffset = new(0.0f, 0.11f, 0.08f);

		public bool IsSmoothed
		{
			get => _isSmoothed;

			set
			{
				_isSmoothed = value;

				if (behaviour != null)
				{
					behaviour.Smoothing = value;
				}
			}
		}

		private bool _isSmoothed = false;

		public OffsetBehaviors OffsetBehavior
		{
			get => _offsetBehavior;

			set
			{
				_offsetBehavior = value;

				if (behaviour != null)
				{
					behaviour.OffsetBehavior = (HandConstraint.SolverOffsetBehavior)(int)value;
				}
			}
		}

		private OffsetBehaviors _offsetBehavior = OffsetBehaviors.Hand;

		public RotationBehaviors RotationBehavior
		{
			get => _rotationBehavior;

			set
			{
				_rotationBehavior = value;

				if (behaviour != null)
				{
					behaviour.RotationBehavior = (HandConstraint.SolverRotationBehavior)(int)value;
				}
			}
		}

		private RotationBehaviors _rotationBehavior = RotationBehaviors.Hand;
	}
}