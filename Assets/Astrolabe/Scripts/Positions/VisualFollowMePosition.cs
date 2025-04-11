using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;

namespace Assets.Astrolabe.Scripts.Positions
{
	public sealed class VisualFollowMePosition : VisualRadialViewPosition, IVisualFollowMePosition
	{
		public VisualFollowMePosition(LogicalFollowMePosition logicalFollowMe) : base(logicalFollowMe)
		{
		}

		public new static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalFollowMePosition), typeof(VisualFollowMePosition));
		}

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);

			window.IsBillboardEnabled = IsEnabled;
			window.BillboardPivotAxis = BillboardPivotAxis.Y;
		}

		public override void OnRemove()
		{
			window.IsBillboardEnabled = false;

			base.OnRemove();
		}

		public override bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;

				if (window != null)
				{
					window.IsBillboardEnabled = value;
				}
			}
		}
	}
}