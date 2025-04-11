using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.IO;
using Astrolabe.Twinkle.Renderers;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Constraints
{
	public abstract class VisualConstraint : VisualObject, IVisualConstraint
	{
		public static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalConstraint), typeof(VisualConstraint));
		}

		protected GameObject gameObject;

		/// <summary>
		/// parent ou est appliqué le VisualConstraint (LogicalManipulator)
		/// </summary>
		/// <param name="parent"></param>
		public void OnAdd(ILogicalElement parent)
		{
			_logicalParent = parent;

			gameObject = parent.GetGameObject();

			// on route le gameobject vers RoundedQuad
			gameObject = gameObject.transform.Find("RoundedQuad")?.gameObject;

			transformConstraint = OnAddOverride(parent, gameObject);

			InitializeProperty();
		}

		public void OnRemove()
		{
			OnRemoveOverride();

			Object.Destroy(transformConstraint);
			transformConstraint = null;
		}

		protected abstract TransformConstraint OnAddOverride(ILogicalElement parent, GameObject gameobject);

		protected virtual void OnRemoveOverride()
		{
		}

		private LogicalConstraint _logicalConstraint;

		// element qui accueillera physiquement les contraintes (LogicalManipulator par exemple)
		private ILogicalElement _logicalParent;

		public VisualConstraint(LogicalConstraint logicalConstraint) : base(logicalConstraint)
		{
			_logicalConstraint = logicalConstraint;
		}

		protected TransformConstraint transformConstraint = null;

		[TwinkleLogicalProperty]
		public virtual bool IsEnabled
		{
			get
			{
				if (transformConstraint == null)
				{
					return IsEnabled;
				}

				return transformConstraint.enabled;
			}

			set
			{
				_isEnabled = value;

				if (transformConstraint != null)
				{
					transformConstraint.enabled = value;
				}
			}
		}

		private bool _isEnabled = true;

		[TwinkleLogicalProperty]
		public ManipulationTypes HandType
		{
			get
			{
				if (transformConstraint == null)
				{
					return _handType;
				}

				return (ManipulationTypes)(int)transformConstraint.HandType;
			}

			set
			{
				_handType = value;

				if (transformConstraint != null)
				{
					transformConstraint.HandType = (ManipulationHandFlags)(int)value;
				}
			}
		}

		private ManipulationTypes _handType = ManipulationTypes.OneHand;

		[TwinkleLogicalProperty]
		public ProximityTypes ProximityType
		{
			get
			{
				if (transformConstraint == null)
				{
					return _proximityType;
				}

				return (ProximityTypes)(int)transformConstraint.ProximityType;
			}

			set
			{
				_proximityType = value;

				if (transformConstraint != null)
				{
					transformConstraint.ProximityType = (ManipulationProximityFlags)(int)value;
				}
			}
		}

		private ProximityTypes _proximityType = ProximityTypes.Far | ProximityTypes.Near;

		/// <summary>
		/// InitializeProperty : Quand la contrainte est ajouté à sa collecton, le TransformConstraint est crée.
		/// Les propriétés sont peut être deja initialisé alors que le TransformConstraint n'existait pas.
		/// On reaffect les valeur au TransformConstraint
		/// </summary>
		protected virtual void InitializeProperty()
		{
			IsEnabled = _isEnabled;
			HandType = _handType;
			ProximityType = _proximityType;
		}
	}
}