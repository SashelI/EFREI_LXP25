using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit.SpatialManipulation;

namespace Assets.Astrolabe.Scripts.Positions
{
	public class VisualRadialViewPosition : VisualSolver<RadialView>, IVisualRadialViewPosition
	{
		public VisualRadialViewPosition(LogicalRadialViewPosition logicalRadialView) : base(logicalRadialView)
		{
		}

		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalRadialViewPosition), typeof(VisualRadialViewPosition));
		}

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);

			behaviour.ReferenceDirection = RadialViewReferenceDirection.GravityAligned;

			// Permet de ne pas remetre à 0 le transform en entrée et sortie
			// ne fonctionne pas (stop le RadiaView)
			//this.behaviour.UpdateLinkedTransform = true;

			MaxViewDegrees = _maxViewDegrees;
			MinViewDegrees = _minViewDegrees;
			MaxDistance = _maxDistance;
			MinDistance = _minDistance;
		}

		public float MaxViewDegrees
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.MaxViewDegrees;
				}

				return _maxViewDegrees;
			}

			set
			{
				if (behaviour != null)
				{
					behaviour.MaxViewDegrees = value;
				}

				_maxViewDegrees = value;
			}
		}

		private float _maxViewDegrees = 30f;

		public float MinViewDegrees
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.MinViewDegrees;
				}

				return _minViewDegrees;
			}

			set
			{
				if (behaviour != null)
				{
					behaviour.MinViewDegrees = value;
				}

				_minViewDegrees = value;
			}
		}

		private float _minViewDegrees = 0f;

		public float MaxDistance
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.MaxDistance.ToLogical();
				}

				return _maxDistance;
			}

			set
			{
				if (behaviour != null)
				{
					behaviour.MaxDistance = value.ToVisual();
				}

				_maxDistance = value;
			}
		}

		private float _maxDistance = 2f;

		public float MinDistance
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.MinDistance.ToLogical();
				}

				return _minDistance;
			}

			set
			{
				if (behaviour != null)
				{
					behaviour.MinDistance = value.ToVisual();
				}

				_minDistance = value;
			}
		}

		private float _minDistance = 0.3f;

		public float MoveLerpTime
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.MoveLerpTime;
				}

				return _moveLerpTime;
			}
			set
			{
				if (behaviour != null)
				{
					behaviour.MoveLerpTime = value;
				}

				_moveLerpTime = value;
			}
		}

		private float _moveLerpTime = 0.1f;

		public float RotateLerpTime
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.RotateLerpTime;
				}

				return _rotateLerpTime;
			}
			set
			{
				if (behaviour != null)
				{
					behaviour.RotateLerpTime = value;
				}

				_rotateLerpTime = value;
			}
		}

		private float _rotateLerpTime = 0.1f;

		public float FixedVerticalPosition
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.FixedVerticalPosition;
				}

				return _fixedVerticalPosition;
			}
			set
			{
				if (behaviour != null)
				{
					behaviour.FixedVerticalPosition = value;
				}

				_fixedVerticalPosition = value;
			}
		}

		private float _fixedVerticalPosition = -0.4f;


		public bool UseFixedVerticalPosition
		{
			get
			{
				if (behaviour != null)
				{
					return behaviour.UseFixedVerticalPosition;
				}

				return _useFixedVerticalPosition;
			}
			set
			{
				if (behaviour != null)
				{
					behaviour.UseFixedVerticalPosition = value;
				}

				_useFixedVerticalPosition = value;
			}
		}

		private bool _useFixedVerticalPosition = false;
	}
}