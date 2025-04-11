using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using Vector3 = Astrolabe.Twinkle.Vector3;

namespace Assets.Astrolabe.Scripts.Positions
{
	public class VisualPosition : VisualObject, IVisualPosition
	{
		protected ILogicalWindow window;

		public static void LinkToVisualFactory()
		{
			VisualObjectFactory.Instance.Add(typeof(LogicalPosition), typeof(VisualPosition));
		}

		private LogicalPosition _logicalPosition;

		public VisualPosition(LogicalPosition logicalPosition) : base(logicalPosition)
		{
			_logicalPosition = logicalPosition;
		}

		protected SolverHandler solverHandler = null;
		protected GameObject gameObject = null;

		public virtual bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Set the position of this window at a distance of another window
		/// Tout les paramètres sont en visual
		/// </summary>
		/// <param name="window"></param>
		/// <param name="distance"></param>
		public void SetDistanceFromWindow(ILogicalWindow window, float x, float y, float distance)
		{
			// Permet d'envoyer une fenetre devant sa maman avec une distance 20 cm
			var transform = window.GetGameObject().transform;
			var childPosition = transform.forward * distance + transform.position;

			childPosition += transform.up * y;
			childPosition += transform.right * x;

			gameObject.transform.position = childPosition;
		}

		public Vector3 GetDistanceFromWindow(ILogicalWindow window, float x, float y, float distance)
		{
			// Permet d'envoyer une fenetre devant sa maman avec une distance 20cm
			var transform = window.GetGameObject().transform;
			var childPosition = transform.forward * distance + transform.position;

			childPosition += transform.up * y;
			childPosition += transform.right * x;

			return childPosition.ToVector3().ToVisual();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="logicalDistance">Exprimé en Visual</param>
		public void SetDistanceFromHead(float distance)
		{
			gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;
		}

		public float GetDistanceFromHead()
		{
			var windowPosition = gameObject.transform.position;
			var cameraPosition = Camera.main.transform.position;

			return UnityEngine.Vector3.Distance(cameraPosition, windowPosition);
		}

		/// <summary>
		/// Coordinate est en coordonnée absolue (monde)
		/// Coordinate ne tient pas compte de IsEnabled à false. Il est toujours actif. Il est toujours exprimé en Visual du coté VisualPosition
		/// </summary>

		public virtual Vector3 Coordinate
		{
			get
			{
				if (gameObject == null)
				{
					return coordinate;
				}

				var v = gameObject.transform.position;

				return new Vector3(v.x, -v.y, v.z);
			}

			set
			{
				if (gameObject != null)
				{
					var v = value;
					gameObject.transform.position = new UnityEngine.Vector3(v.X, -v.Y, v.Z);
				}

				coordinate = value;
			}
		}

		// toujours en visual
		protected Vector3 coordinate = Vector3.Zero;

		public Vector3 Translation
		{
			get
			{
				if (solverHandler == null)
				{
					return _translation;
				}

				var v = solverHandler.AdditionalOffset;

				return new Vector3(v.x, -v.y, v.z);
			}

			set
			{
				if (solverHandler != null)
				{
					var v = value;
					solverHandler.AdditionalOffset = new UnityEngine.Vector3(v.X, -v.Y, v.Z);
				}

				_translation = value;
			}
		}

		private Vector3 _translation;

		public Vector3 Rotation
		{
			get
			{
				if (solverHandler == null)
				{
					return _rotation;
				}

				var v = solverHandler.AdditionalRotation;

				return new Vector3(v.x, v.y, v.z);
			}

			set
			{
				if (solverHandler != null)
				{
					var v = value;
					solverHandler.AdditionalRotation = new UnityEngine.Vector3(v.X, v.Y, v.Z);
				}

				_rotation = value;
			}
		}

		private Vector3 _rotation;

		public virtual void OnAdd(ILogicalWindow window)
		{
			this.window = window;
			gameObject = window.GetGameObject();

			solverHandler = gameObject.GetComponent<SolverHandler>();

			Translation = Translation;
			Rotation = Rotation;

			Coordinate = coordinate;
		}

		public virtual void OnRemove()
		{
			window = null;
		}
	}

	/// <summary>
	/// Solver qui utilise un Component.
	/// Le problème c'est que les components sont souvent partagés entre Billboard/RadialView et TagAlong par exemple
	/// On doit donc supprimé les Component uniquement si aucun Solver n'en a besoin.
	/// </summary>
	public abstract class VisualPosition<TBehaviour> : VisualPosition where TBehaviour : Behaviour
	{
		public VisualPosition(LogicalPosition logicalPosition) : base(logicalPosition)
		{
		}

		protected TBehaviour behaviour;

		public TBehaviour Behaviour => behaviour;

		public override bool IsEnabled
		{
			get => base.IsEnabled;

			set
			{
				base.IsEnabled = value;

				if (behaviour == null)
				{
					return;
				}

				Coordinate = coordinate;
				behaviour.enabled = value;
			}
		}

		public override void OnAdd(ILogicalWindow window)
		{
			base.OnAdd(window);
			behaviour = AddOrEnableBehaviour();
		}

		public override void OnRemove()
		{
			DisableBehaviour(behaviour);
			base.OnRemove();
		}

		/// <summary>
		/// On ajoute ou on reactive le behaviour
		/// </summary>
		/// <typeparam name="TBehaviour"></typeparam>
		/// <param name="window"></param>
		/// <returns></returns>
		protected TBehaviour AddOrEnableBehaviour()
		{
			var gameObject = window.GetGameObject();
			var behaviour = gameObject.GetOrAddComponent<TBehaviour>();

			behaviour.enabled = IsEnabled;

			return behaviour;
		}

		/// <summary>
		/// On desactive le behaviour
		/// </summary>
		/// <param name="behaviour"></param>
		protected void DisableBehaviour(Behaviour behaviour)
		{
			if (behaviour != null)
			{
				behaviour.enabled = false;
			}
		}

		protected override void DisposeOverride()
		{
			Dispose();

			if (behaviour != null)
			{
				Object.Destroy(behaviour);
				behaviour = null;
			}
		}
	}
}