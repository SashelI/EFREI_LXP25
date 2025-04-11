using Astrolabe.Twinkle;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Positions
{
	public abstract class VisualSolver<TBehaviour> : VisualPosition<TBehaviour> where TBehaviour : Behaviour
	{
		public VisualSolver(LogicalPosition logicalPosition) : base(logicalPosition)
		{
		}

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);

			solverHandler = gameObject.GetComponent<SolverHandler>();
			solverHandler.enabled = IsEnabled;
		}


		public override bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;

				if (solverHandler != null)
				{
					solverHandler.enabled = value;
				}
			}
		}
	}
}