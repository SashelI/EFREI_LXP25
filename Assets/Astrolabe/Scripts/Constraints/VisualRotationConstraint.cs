using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Constraints
{
	public class VisualRotationConstraint : VisualConstraint, IVisualRotationConstraint
	{
		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalRotationConstraint), typeof(VisualRotationConstraint));
		}

		private RotationAxisConstraint _rotationAxisConstraint;

		/// <summary>
		/// Ajoute de la contrainte dans le gameobject
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="gameobject"></param>
		/// <returns></returns>
		protected override TransformConstraint OnAddOverride(ILogicalElement parent, GameObject gameobject)
		{
			_rotationAxisConstraint = gameObject?.AddComponent<RotationAxisConstraint>();
			return _rotationAxisConstraint;
		}

		protected override void InitializeProperty()
		{
			base.InitializeProperty();

			RotationAxis = _rotationAxis;
		}

		public VisualRotationConstraint(LogicalRotationConstraint logicalRotationConstraint) : base(
			logicalRotationConstraint)
		{
		}

		public TransformAxis RotationAxis
		{
			get
			{
				if (transformConstraint == null)
				{
					return _rotationAxis;
				}

				return (TransformAxis)(int)_rotationAxisConstraint.ConstraintOnRotation;
			}

			set
			{
				_rotationAxis = value;

				if (transformConstraint != null)
				{
					_rotationAxisConstraint.ConstraintOnRotation = (AxisFlags)(int)value;
				}
			}
		}

		private TransformAxis _rotationAxis = TransformAxis.All;
	}
}