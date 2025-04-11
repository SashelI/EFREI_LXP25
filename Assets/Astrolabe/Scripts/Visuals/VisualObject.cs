using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public abstract class VisualObject : IVisualObject
	{
		public VisualObject(ILogicalObject logicalObject)
		{
			LogicalObject = logicalObject;
		}

		public virtual object Element { get; }

		public ILogicalObject LogicalObject { get; protected set; }

		public void Dispose()
		{
			DisposeOverride();
		}

		protected virtual void DisposeOverride()
		{
		}
	}
}