using Assets.Astrolabe.Scripts.Components;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualRectangle : VisualFluentElement, IVisualRectangle
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalRectangle), typeof(VisualRectangle));
		}

		public VisualRectangle(ILogicalElement logicalElement) : base(logicalElement)
		{
			rectangle = Object.Instantiate(TwinklePrefabFactory.Instance.prefabRectangle).GetComponent<LayoutRoundedRectangle>();

			FluentShader = rectangle.FluentShader;
			TwinkleComponent = rectangle;
		}

		protected LayoutRoundedRectangle rectangle;

		public CornerRadius CornerRadius
		{
			get => _cornerRadius;

			set
			{
				if (_cornerRadius != value)
				{
					_cornerRadius = value;
					rectangle.CornerRadius = value;
				}
			}
		}

		private CornerRadius _cornerRadius;

		public SolidColorBrush BorderBrush
		{
			get => _borderBrush;

			set
			{
				if (_borderBrush != value)
				{
					_borderBrush = value;

					rectangle.ChangeBorderProperties(value, _borderThickness, Opacity);
				}
			}
		}

		private SolidColorBrush _borderBrush;

		public float BorderThickness
		{
			get => _borderThickness;

			set
			{
				if (_borderThickness != value)
				{
					_borderThickness = value;

					rectangle.ChangeBorderProperties(_borderBrush, value, Opacity);
				}
			}
		}

		private float _borderThickness;

		public float BorderForward
		{
			get => _borderForward;

			set
			{
				if (_borderForward != value)
				{
					_borderForward = value;

					rectangle.SetBorderForward(value);
				}
			}
		}

		private float _borderForward;

		public override float Opacity
		{
			get => base.Opacity;

			set
			{
				// opacity de FluentShader de VisualFluentElement
				base.Opacity = value;
				rectangle.ChangeBorderProperties(_borderBrush, _borderThickness, value);
			}
		}

		public TouchStrategy TouchStrategy
		{
			get
			{
				var manipulator = rectangle.GetComponent<ObjectManipulator>();

				if (manipulator != null && manipulator.AllowedInteractionTypes.HasFlag(InteractionFlags.Near))
				{
					return TouchStrategy.FarAndNear;
				}

				return TouchStrategy.Far;
			}

			set
			{
				var manipulator = rectangle.GetComponent<ObjectManipulator>();

				if (TouchStrategy == TouchStrategy.Far)
				{
					if (manipulator != null)
					{
						manipulator.AllowedInteractionTypes = InteractionFlags.Ray;
					}
				}
				else
				{
					if (manipulator == null)
					{
						manipulator = rectangle.gameObject.AddComponent<ObjectManipulator>();
					}

					manipulator.AllowedInteractionTypes = InteractionFlags.Ray | InteractionFlags.Near;
				}
			}
		}
	}
}