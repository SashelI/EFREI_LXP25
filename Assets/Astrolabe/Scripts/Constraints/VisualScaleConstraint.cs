using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Constraints
{
	public class VisualScaleConstraint : VisualConstraint, IVisualScaleConstraint
	{
		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalScaleConstraint), typeof(VisualScaleConstraint));
		}

		private MinMaxScaleConstraint _minMaxScaleConstraint;

		/// <summary>
		/// Ajoute de la contrainte dans le gameobject
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="gameobject"></param>
		/// <returns></returns>
		protected override TransformConstraint OnAddOverride(ILogicalElement parent, GameObject gameobject)
		{
			_minMaxScaleConstraint = gameObject.AddComponent<MinMaxScaleConstraint>();
			return _minMaxScaleConstraint;
		}

		protected override void InitializeProperty()
		{
			base.InitializeProperty();

			Minimum = _minimum;
			Maximum = _maximum;
		}

		public VisualScaleConstraint(LogicalScaleConstraint logicalScaleConstraint) : base(logicalScaleConstraint)
		{
		}

		public float Minimum
		{
			get
			{
				if (transformConstraint == null)
				{
					return _minimum;
				}

				return _minMaxScaleConstraint.MinimumScale.x.ToLogical();
			}

			set
			{
				_minimum = value;

				if (transformConstraint != null)
				{
					_minMaxScaleConstraint.MinimumScale =
						new UnityEngine.Vector3(value.ToVisual(), value.ToVisual(), value.ToVisual());
				}
			}
		}

		private float _minimum = 0.2f; // en logic

		public float Maximum
		{
			get
			{
				if (transformConstraint == null)
				{
					return _maximum;
				}

				return _minMaxScaleConstraint.MaximumScale.x.ToLogical();
			}

			set
			{
				_maximum = value;

				if (transformConstraint != null)
				{
					_minMaxScaleConstraint.MaximumScale =
						new UnityEngine.Vector3(value.ToVisual(), value.ToVisual(), value.ToVisual());
				}
			}
		}

		private float _maximum = 2f; // en logic
	}
}