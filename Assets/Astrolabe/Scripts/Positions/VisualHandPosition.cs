using System;
using System.Threading.Tasks;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;

namespace Assets.Astrolabe.Scripts.Positions
{
	public class VisualHandPosition : VisualSolver<HandConstraintPalmUp>, IVisualHandPosition
	{
		private readonly LogicalHandPosition _logicalHand;

		public VisualHandPosition(LogicalHandPosition logicalHand) : base(logicalHand)
		{
			_logicalHand = logicalHand;
		}

		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalHandPosition), typeof(VisualHandPosition));
		}

		public event EventHandler HandActivated;

		public event EventHandler HandDesactivated;

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);

			solverHandler.TrackedTargetType = TrackedObjectType.HandJoint;
			solverHandler.TrackedHandJoint = TrackedHandJoint.Palm;
			behaviour.UpdateWhenOppositeHandNear = true;

			Hand = _hand;
			Placement = _placement;
			PlacementOffset = _placementOffset;
			DisparitionDelay = _disparitionDelay;

			RotationBehavior = _rotationBehavior;
			OffsetBehavior = _offsetBehavior;

			IsSmoothed = _isSmoothed;

			IsFlatHandRequired = _isFlatHandRequired;

			behaviour.OnHandActivate.AddListener(OnHandActivate);
			behaviour.OnHandDeactivate.AddListener(OnHandDesactivate);

			if (AutoHide)
			{
				window.VisualElement.RenderTransformVisibility = Visibility.Collapsed;
				//((LogicalElement)this.Window).Scale = Vector3.Zero;
			}
		}

		public override void OnRemove()
		{
			// On remet la valeur par défaut
			solverHandler.TrackedTargetType = TrackedObjectType.Head;

			if (AutoHide)
			{
				window.VisualElement.RenderTransformVisibility = Visibility.Visible;
				//((LogicalElement)this.Window).Scale = Vector3.Identity;
			}

			base.OnRemove();
		}

		public override bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				var coordinate = Coordinate;

				base.IsEnabled = value;
				IsHandActivated = false;

				if (value == false)
				{
					// On replace les coordonnées de la main car sinon il repasse à zero automatiquement dès qu'ils sont disable
					Coordinate = coordinate;
				}
				else
				{
					if (solverHandler.AdditionalOffset !=
					    UnityEngine.Vector3
						    .zero) //Si l'on vient d'une manette et que le solver n'a pas été nettoyé, on remet les valeurs
					{
						solverHandler.AdditionalOffset = UnityEngine.Vector3.zero;
					}
				}
			}
		}

		public bool IsHandActivated { get; private set; }

		private void OnHandActivate()
		{
			if (IsEnabled)
			{
				if (AutoHide)
				{
					window.VisualElement.RenderTransformVisibility = Visibility.Visible;
					//((LogicalElement)this.Window).Scale = Vector3.Identity;
				}

				IsHandActivated = true;
				HandActivated?.Invoke(_logicalHand, EventArgs.Empty);
			}
		}

		private async void OnHandDesactivate()
		{
			if (IsEnabled)
			{
				if (AutoHide)
				{
					await HideAsync(_disparitionDelay);
				}

				IsHandActivated = false;
				HandDesactivated?.Invoke(_logicalHand, EventArgs.Empty);
			}
		}

		private async Task HideAsync(int delay)
		{
			await Task.Delay(delay);
			window.VisualElement.RenderTransformVisibility = Visibility.Collapsed;
		}

		public bool IsFlatHandRequired
		{
			get { return _isFlatHandRequired; }

			set
			{
				_isFlatHandRequired = value;

#if !UNITY_EDITOR
                if(this.behaviour != null)
                {
                    // Empeche la main de fonctionner sur l'éditeur
                    this.behaviour.RequireFlatHand = true;
                }
#endif
			}
		}

		private bool _isFlatHandRequired = false;

		public Hands Hand
		{
			get => _hand;

			set
			{
				_hand = value;

				if (solverHandler != null)
				{
					solverHandler.TrackedHandedness = (Handedness)(int)value;
				}
			}
		}

		private Hands _hand = Hands.None;

		public HandPlacements Placement
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

		private HandPlacements _placement = HandPlacements.LittleFinger;

		public float PlacementOffset
		{
			get => _placementOffset;

			set
			{
				_placementOffset = value;

				if (behaviour != null)
				{
					behaviour.SafeZoneBuffer = value;
				}
			}
		}

		private float _placementOffset = 0.15f;

		public int DisparitionDelay
		{
			get => _disparitionDelay;

			set => _disparitionDelay = value;
		}

		private int _disparitionDelay = 0;

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

		public bool AutoHide { get; set; } = true;

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