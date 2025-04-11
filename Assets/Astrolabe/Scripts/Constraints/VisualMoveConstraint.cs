using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Constraints
{
	public class VisualMoveConstraint : VisualConstraint, IVisualMoveConstraint
	{
		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalMoveConstraint), typeof(VisualMoveConstraint));
		}

		private MoveAxisConstraint _moveAxisConstraint;

		/// <summary>
		/// Ajoute de la contrainte dans le gameobject
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="gameobject"></param>
		/// <returns></returns>
		protected override TransformConstraint OnAddOverride(ILogicalElement parent, GameObject gameobject)
		{
			_moveAxisConstraint = gameObject.AddComponent<MoveAxisConstraint>();
			return _moveAxisConstraint;
		}

		protected override void InitializeProperty()
		{
			base.InitializeProperty();

			MoveAxis = _moveAxis;
		}

		public VisualMoveConstraint(LogicalMoveConstraint logicalMoveConstraint) : base(logicalMoveConstraint)
		{
		}

		public TransformAxis MoveAxis
		{
			get
			{
				if (transformConstraint == null)
				{
					return _moveAxis;
				}

				return (TransformAxis)(int)_moveAxisConstraint.ConstraintOnMovement;
			}

			set
			{
				_moveAxis = value;

				if (transformConstraint != null)
				{
					_moveAxisConstraint.ConstraintOnMovement = (AxisFlags)(int)value;
				}
			}
		}

		private TransformAxis _moveAxis = TransformAxis.All;
	}
}