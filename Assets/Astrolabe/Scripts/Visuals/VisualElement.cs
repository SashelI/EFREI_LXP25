using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using UnityEngine;
using Vector3 = Astrolabe.Twinkle.Vector3;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public abstract class VisualElement : VisualObject, IVisualElement
	{
		// ici mettre le Control qui permet de stocker des enfants
		private TwinkleComponent _twinkleComponent;

		private AstrolabeBillboard _billboard;

		public VisualElement(ILogicalElement logicalElement) : base(logicalElement)
		{
			LogicalElement = logicalElement;
		}

		// Element logique
		public ILogicalElement LogicalElement
		{
			get => _logicalElement;

			private set
			{
				_logicalElement = value;
				IsLogicalWindow = value is ILogicalWindow;
			}
		}

		private ILogicalElement _logicalElement;

		public bool IsLogicalWindow { get; private set; }

		public override object Element => _twinkleComponent;

		public TwinkleComponent TwinkleComponent
		{
			get => _twinkleComponent;

			protected set
			{
				_twinkleComponent = value;

				if (value != null)
				{
					value.VisualElement = this;
					value.transform.name = value.name + LogicalElement;

					// Is this a child of a 2D Window
					bool is2DElement = ((LogicalElement)LogicalElement).GetWindow() is LogicalWindow2D;

					if (is2DElement)
					{
						//Si la window est en 2D, on a besoin d'update la scale
						_twinkleComponent.Scale = Scale;
					}
				}
			}
		}

		/*
        public static void SetHitTestVisible(TwinkleComponent rootComponent, bool value)
        {
            var colliders = rootComponent.GetComponentsInChildren<BoxCollider>();

            if (colliders != null)
            {
                foreach (var collider in colliders)
                {
                    collider.enabled = value;
                }
            }

            var nearInteractionTouchables = rootComponent.GetComponentsInChildren<NearInteractionTouchable>();

            if (nearInteractionTouchables != null)
            {
                foreach (var nearInteractionTouchable in nearInteractionTouchables)
                {
                    nearInteractionTouchable.enabled = value;
                }
            }
        }
        */

		/// <summary>
		/// a overrider
		/// </summary>
		/// <param name="value"></param>
		public virtual void SetHitTestVisible(bool value)
		{
			isHitTestVisible = value;
		}

		protected bool isHitTestVisible = true;

		public void SetVisibility(Visibility visibility)
		{
			if (_twinkleComponent != null)
			{
				_twinkleComponent.gameObject.SetActive(visibility == Visibility.Visible);
			}
		}

		public IVisualElement VisualParent
		{
			get
			{
				// On prend ParentTree car ParentElement ne permet pas de rentrer à l'intérieur des templates et des controles presenter
				var logicalParent = LogicalElement?.ParentTree as ILogicalElement;
				return logicalParent?.VisualElement;
			}
		}

		public Vector3 Rotation
		{
			get => _twinkleComponent.Rotation;

			set => _twinkleComponent.Rotation = value;
		}

		public Vector3 Scale
		{
			get => _twinkleComponent.Scale;

			set => _twinkleComponent.Scale = value;
		}

		public Vector3 Translation
		{
			get => _twinkleComponent.Translation;

			set => _twinkleComponent.Translation = value;
		}

		/// <summary>
		/// Coordonnée correspondant à localposition de l'objet le plus haut de VisualElement (comme Window.Position.Coordinate mais en lecture pour ne pas casser le layout)
		/// </summary>

		public Vector3 LocalCoordinate => _twinkleComponent.transform.localPosition.ToVector3().ToVisual();

		public Vector3 Pivot
		{
			get => _twinkleComponent.Pivot;

			set => _twinkleComponent.Pivot = value;
		}

		public Vector3 Angle
		{
			get => _twinkleComponent.transform.localEulerAngles.ToVector3();
			//var angle = this.twinkleComponent.transform.localEulerAngles;
			//return new Vector3(angle.x, angle.y, -angle.z);
			set =>
				//var angle = new UnityEngine.Vector3(value.X, value.Y, -value.Z);
				//this.twinkleComponent.transform.localEulerAngles = angle;  //new UnityEngine.Vector3(value.X, value.Y, -value.Z);
				_twinkleComponent.transform.localEulerAngles = value.ToVector3();
		}

		public Vector3 Zoom
		{
			get => _twinkleComponent.transform.localScale.ToVector3();

			set => _twinkleComponent.transform.localScale = new UnityEngine.Vector3(value.X, value.Y, value.Z);
		}

		public void AddChild(IVisualElement child)
		{
			if (child != null)
			{
				_twinkleComponent.AddChild(child);
			}
		}

		public void RemoveChild(IVisualElement child)
		{
			if (child != null)
			{
				_twinkleComponent.RemoveChild(child);
			}
		}

		protected void Locate(Vector3 relativeLocation)
		{
			// ici on positionne le control
			_twinkleComponent.Left = relativeLocation.X;
			_twinkleComponent.Top = relativeLocation.Y;
			_twinkleComponent.Forward = relativeLocation.Z;
		}

		protected override void DisposeOverride()
		{
			Object.Destroy(_twinkleComponent.gameObject);
		}

		public virtual void Resize(Box box)
		{
			_twinkleComponent.Width = box.Width;
			_twinkleComponent.Height = box.Height;
			_twinkleComponent.Depth = box.Depth;

			Locate(box.Location);

			ResizeOverride(box);
		}

		protected virtual void ResizeOverride(Box box)
		{
		}

		public void SetIsBillboardEnabled(bool isEnabled)
		{
			var gameObject = LogicalElement?.GetGameObject();

			if (gameObject != null)
			{
				if (isEnabled == true)
				{
					var behaviour = gameObject.GetOrAddComponent<AstrolabeBillboard>();

					behaviour.enabled = true;
					_billboard = behaviour;
				}
				else
				{
					if (_billboard != null)
					{
						_billboard.enabled = false;
					}
				}
			}
		}

		public void SetBillboardPivotAxis(BillboardPivotAxis billboardPivotAxis)
		{
			if (_billboard != null)
			{
				_billboard.PivotAxis = (PivotAxis)(int)billboardPivotAxis;
			}
		}

		public void SetBillboardParentRotationLocked(bool isParentRotationLocked)
		{
			if (_billboard != null)
			{
				_billboard.isParentRotationLocked = isParentRotationLocked;
			}
		}

		/// <summary>
		/// Force la creation du RenderTransform
		/// </summary>
		public void EnsureRenderTransformExists()
		{
			TwinkleComponent.EnsureRenderTransformExists();
		}

		public Visibility RenderTransformVisibility
		{
			get
			{
				var isActivated = TwinkleComponent.EnsureRenderTransformExists().gameObject.activeSelf;

				return isActivated ? Visibility.Visible : Visibility.Collapsed;
			}

			set => TwinkleComponent.EnsureRenderTransformExists().gameObject.SetActive(value == Visibility.Visible);
		}

		/// <summary>
		/// Sera utilisé dans VisualFluentElement et VisualModel3D pour forcer le type de shader à utiliser
		/// </summary>
		/*
        public virtual ShaderMode ShaderMode
        {
            get;
            set;
        }
        */
		public void RefreshName(string name)
		{
			var go = LogicalElement.GetGameObject();

			if (go != null)
			{
				go.name = _logicalElement.ToString();
			}
		}

		public float GetDistanceFromHead(bool includeRenderTransformation)
		{
			var cameraPosition = Camera.main.transform.position;

			Transform t1;

			if (includeRenderTransformation == false)
			{
				t1 = LogicalElement.GetGameObject().transform;
			}
			else
			{
				t1 = LogicalElement.GetRenderTransform();

				if (t1 == null)
				{
					t1 = LogicalElement.GetGameObject().transform;
				}
			}

			return UnityEngine.Vector3.Distance(t1.position, cameraPosition).ToLogical();
		}

		public float GetDistance(ILogicalElement logicalElementToCompare, bool includeRenderTransformation)
		{
			Transform t1;
			Transform t2;

			if (includeRenderTransformation == false)
			{
				t1 = LogicalElement.GetGameObject().transform;
				t2 = logicalElementToCompare.GetGameObject().transform;
			}
			else
			{
				t1 = LogicalElement.GetRenderTransform();

				if (t1 == null)
				{
					t1 = LogicalElement.GetGameObject().transform;
				}

				t2 = logicalElementToCompare.GetRenderTransform();

				if (t2 == null)
				{
					t2 = logicalElementToCompare.GetGameObject().transform;
				}
			}

			return UnityEngine.Vector3.Distance(t1.position, t2.position).ToLogical();
		}

		public Vector3 GetRelativePositionToElement(ILogicalElement logicalElementToCompare,
			bool includeRenderTransformation)
		{
			Transform t1;
			Transform t2;

			if (includeRenderTransformation == false)
			{
				t1 = LogicalElement.GetGameObject().transform;
				t2 = logicalElementToCompare.GetGameObject().transform;
			}
			else
			{
				t1 = LogicalElement.GetRenderTransform();

				if (t1 == null)
				{
					t1 = LogicalElement.GetGameObject().transform;
				}

				t2 = logicalElementToCompare.GetRenderTransform();

				if (t2 == null)
				{
					t2 = logicalElementToCompare.GetGameObject().transform;
				}
			}

			var v2 = t1.InverseTransformPoint(t2.position);

			return new Vector3(
				v2.x.ToLogical(),
				(-v2.y).ToLogical(), // p1 - p2 pour compatibilité Logical
				v2.z.ToLogical());
		}
	}
}