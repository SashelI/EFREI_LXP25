using System;
using Assets.Astrolabe.Scripts.Tools;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Assets.Astrolabe.Scripts.Components
{
	public abstract class TwinkleComponent : MonoBehaviour
	{
		protected Transform renderTransform;
		protected Transform renderTransformRotationScale;
		protected Transform renderTransformOrigin;

		protected XRGrabInteractable nearInteractionGrabbable;
		protected BoxCollider nearInteractionGrabbableCollider;

		private Transform _childrenContainer;

		private PixelConverter _pixelConverter;

		private bool _isLogicalWindow = false;
		public bool is2DElement { get; protected set; } = false;

		public BoxCollider NearInteractionGrabbableCollider
		{
			get => nearInteractionGrabbableCollider;

			set => nearInteractionGrabbableCollider = value;
		}

		public VisualElement VisualElement
		{
			get => _visualElement;

			set
			{
				_visualElement = value;

				if (value != null)
				{
					_isLogicalWindow = value.IsLogicalWindow;

					Initialize();
				}
			}
		}

		private VisualElement _visualElement;

		public float Forward
		{
			get => _forward;

			set
			{
				if (_forward != value)
				{
					SetForward(value);
				}
			}
		}

		private float _forward = float.NaN;

		protected void SetForward(float value)
		{
			if (_visualElement == null)
			{
				return;
			}

			_forward = value;

			if (_isLogicalWindow == true)
			{
				return;
			}

			if (float.IsNaN(value))
			{
				value = 0;
			}

			float d = 0;

			if (float.IsNaN(_forward) == false)
			{
				d = _forward;
			}

			SetForwardOverride(d);
		}

		protected virtual void SetForwardOverride(float forward)
		{
			var position = transform.localPosition;
			var newPosition = new UnityEngine.Vector3(position.x, position.y, -forward);
			transform.localPosition = newPosition;
		}

		public float Left
		{
			get => _left;

			set
			{
				if (_left != value)
				{
					SetLeft(value);
				}
			}
		}

		private float _left = float.NaN;

		protected void SetLeft(float value)
		{
			if (_visualElement == null)
			{
				return;
			}

			_left = value;

			// cas d'une fenetre par exemple
			// Elle n'a pas de parent et est centré sur elle même
			// C'est important que le Set ne s'effectue pas notammement dans le cas d'un RadialView/FollowMe car il modifie sa position ce u(il n'aime pas du tout
			// En revanche on perd le Margin (ce n'est pas grave sur une Window car le comportement est similaire en WPF/UWP)

			if (_isLogicalWindow == true)
			{
				return;
			}

			var parent = VisualElement.VisualParent;

			if (parent == null)
			{
				return;
			}

			if (float.IsNaN(value))
			{
				value = 0;
			}

			var position = transform.localPosition;

			float w = 0;

			if (float.IsNaN(_width) == false)
			{
				w = _width;
			}

			//float parentWidth = 0;
			//if(parent != null)
			//{
			//    parentWidth = pixelConverter.ToVisual(parent.LogicalElement.RenderSize.Width);
			//}

			var parentWidth = _pixelConverter.ToVisual(parent.LogicalElement.RenderSize.Width);

			var newPosition = new UnityEngine.Vector3(value + (w - parentWidth) / 2, position.y, position.z);

			transform.localPosition = newPosition;
		}

		public float Top
		{
			get => _top;

			set
			{
				if (_top != value)
				{
					SetTop(value);
				}
			}
		}

		private float _top = float.NaN;

		protected void SetTop(float value)
		{
			if (_visualElement == null)
			{
				return;
			}

			_top = value;

			// cas d'une fenetre par exemple
			// Elle n'a pas de parent et est centré sur elle même
			// C'est important que le Set ne s'eefectue pas notammement dans le cas d'un TagAlong

			if (_isLogicalWindow == true)
			{
				return;
			}

			var parent = VisualElement.VisualParent;

			if (parent == null)
			{
				return;
			}

			if (float.IsNaN(value))
			{
				value = 0;
			}

			var position = transform.localPosition;

			float h = 0;

			if (float.IsNaN(_height) == false)
			{
				h = _height;
			}

			//float parentHeight = 0;

			//if(parent != null)
			//{
			//    parentHeight = pixelConverter.ToVisual(parent.LogicalElement.RenderSize.Height);
			//}

			var parentHeight = _pixelConverter.ToVisual(parent.LogicalElement.RenderSize.Height);

			var newPosition = new UnityEngine.Vector3(position.x, -(value + (h - parentHeight) / 2), position.z);

			transform.localPosition = newPosition;
		}

		public float Width
		{
			get => _width;

			set => SetWidth(value);
		}

		private float _width = float.NaN;

		protected void SetWidth(float value)
		{
			_width = value;

			if (float.IsNaN(value) == false)
			{
				SetWidthOverride(value);
				SetPivot(_pivot);

				if (nearInteractionGrabbableCollider != null)
				{
					var size = nearInteractionGrabbableCollider.size;

					nearInteractionGrabbableCollider.size = new UnityEngine.Vector3(
						value,
						size.y,
						size.z
					);
				}
			}

			// on replace left selon la taille
			SetLeft(_left);
		}

		protected virtual void SetWidthOverride(float width)
		{
		}

		public float Height
		{
			get => _height;

			set =>
				// ne surtout ps faire de Leazyloading car cela peut casser le positionnement du top dans le cas ou la taille du parent à changer mais pas la taille du control courant
				// par exemple : Dans un StackPanel (parent) on rend Visible un element Collapsed, la taille de stackpanel change et donc la position des controles également même si leur taille ne change pas
				SetHeight(value);
		}

		private float _height = float.NaN;

		protected void SetHeight(float value)
		{
			_height = value;

			if (float.IsNaN(value) == false)
			{
				SetHeightOverride(value);
				SetPivot(_pivot);

				if (nearInteractionGrabbableCollider != null)
				{
					var size = nearInteractionGrabbableCollider.size;

					nearInteractionGrabbableCollider.size = new UnityEngine.Vector3(
						size.x,
						value,
						size.z
					);
				}
			}

			// on replace left selon la taille
			SetTop(_top);
		}

		protected virtual void SetHeightOverride(float height)
		{
		}

		public float Depth
		{
			get => _depth;

			set => SetDepth(value);
		}

		private float _depth = float.NaN;

		protected void SetDepth(float value)
		{
			_depth = value;

			if (float.IsNaN(value) == false)
			{
				SetDepthOverride(value);

				if (nearInteractionGrabbableCollider != null)
				{
					var size = nearInteractionGrabbableCollider.size;

					nearInteractionGrabbableCollider.size = new UnityEngine.Vector3(
						size.x,
						size.y,
						value
					);
				}
			}
		}

		protected virtual void SetDepthOverride(float depth)
		{
		}

		public Transform EnsureRenderTransformExists()
		{
			if (renderTransform == null)
			{
				renderTransform = new GameObject("RenderTransform").transform;

				renderTransformRotationScale = new GameObject("RenderTransformRotationScale").transform;
				renderTransformRotationScale.transform.SetParent(renderTransform, false);

				renderTransformOrigin = new GameObject("RenderTransformOrigin").transform;
				renderTransformOrigin.transform.SetParent(renderTransformRotationScale, false);

				// copie les enfants dans le renderTransform

				while (transform.childCount != 0)
				{
					var child = transform.GetChild(0);
					child.SetParent(renderTransformOrigin, false);
				}

				renderTransform.transform.SetParent(transform, false);
			}

			return renderTransform;
		}

		public global::Astrolabe.Twinkle.Vector3 Rotation
		{
			get
			{
				if (renderTransform == null)
				{
					return _rotation;
				}
				else
				{
					var a = renderTransformRotationScale.localEulerAngles;
					return new global::Astrolabe.Twinkle.Vector3(a.x, a.y, -a.z);
				}
			}

			set
			{
				var finalRotation = value;

				// Is this a child of a 2D Window
				bool is2DElement = ((LogicalElement)_visualElement.LogicalElement).GetWindow() is LogicalWindow2D;

				if (is2DElement)
				{
					//Si la window est en 2D, nous n'avons qu'un seul degré de liberté
					finalRotation = new global::Astrolabe.Twinkle.Vector3(0, 0, finalRotation.Z);
				}

				_rotation = finalRotation;

				EnsureRenderTransformExists();
				renderTransformRotationScale.localEulerAngles = new UnityEngine.Vector3(finalRotation.X, finalRotation.Y, -finalRotation.Z);
			}
		}

		private global::Astrolabe.Twinkle.Vector3 _rotation = new(0, 0, 0);

		public virtual global::Astrolabe.Twinkle.Vector3 Scale
		{
			get
			{
				if (renderTransform == null)
				{
					return _scale;
				}
				else
				{
					return renderTransformRotationScale.localScale.ToVector3();
				}
			}

			set
			{
				var finalScale = value;

				// Is this a child of a 2D Window
				bool is2DElement = ((LogicalElement)_visualElement.LogicalElement).GetWindow() is LogicalWindow2D;

				if (is2DElement)
				{
					//Si la window est en 2D, les éléments seront trop petits
					finalScale *= 1000;
				}

				_scale = finalScale;

				EnsureRenderTransformExists();

				renderTransformRotationScale.localScale = new UnityEngine.Vector3(finalScale.X, finalScale.Y, finalScale.Z);
			}
		}

		private global::Astrolabe.Twinkle.Vector3 _scale = new(1, 1, 1);

		public virtual global::Astrolabe.Twinkle.Vector3 Pivot
		{
			get => _pivot;

			set => SetPivot(value);
		}

		private global::Astrolabe.Twinkle.Vector3 _pivot = new(0.5f, 0.5f, 0.5f);

		private void SetPivot(global::Astrolabe.Twinkle.Vector3 value)
		{
			_pivot = value;

			if (value.X != 0.5 || value.Y != 0.5 || value.Z != 0.5)
			{
				if (global::Astrolabe.Twinkle.Vector3.IsNaN(value, true))
				{
					return;
				}

				EnsureRenderTransformExists();

				var x = float.IsNaN(_width) ? 0 : (value.X - 0.5f) * _width;
				var y = float.IsNaN(_height) ? 0 : -(value.Y - 0.5f) * _height;
				var z = float.IsNaN(_depth) ? 0 : (value.Z - 0.5f) * _depth;

				var v = new UnityEngine.Vector3(x, y, z);

				renderTransformRotationScale.localPosition = v;
				renderTransformOrigin.localPosition = new UnityEngine.Vector3(-x, -y, -z);
			}
		}

		public global::Astrolabe.Twinkle.Vector3 Translation
		{
			get
			{
				if (renderTransform == null)
				{
					return _translation;
				}
				else
				{
					var v = renderTransform.localPosition;
					return new global::Astrolabe.Twinkle.Vector3(v.x, -v.y, -v.z);
				}
			}

			set
			{
				var finalTranslation = value;

				// Is this a child of a 2D Window
				bool is2DElement = ((LogicalElement)_visualElement.LogicalElement).GetWindow() is LogicalWindow2D;

				if (is2DElement)
				{
					//Si la window est en 2D, seulement deux degrés de liberté
					finalTranslation = new global::Astrolabe.Twinkle.Vector3(finalTranslation.X, finalTranslation.Y, 0);
				}

				_translation = finalTranslation;

				EnsureRenderTransformExists();

				renderTransform.localPosition = new UnityEngine.Vector3(finalTranslation.X, -finalTranslation.Y, -finalTranslation.Z);
			}
		}

		private global::Astrolabe.Twinkle.Vector3 _translation = global::Astrolabe.Twinkle.Vector3.Zero;

		public global::Astrolabe.Twinkle.Vector3 TranslationAfterRotation
		{
			get
			{
				if (renderTransform == null)
				{
					return _translationAfterRotation;
				}
				else
				{
					var v = renderTransformRotationScale.localPosition;
					return new global::Astrolabe.Twinkle.Vector3(v.x, -v.y, -v.z);
				}
			}

			set
			{
				_translationAfterRotation = value;

				EnsureRenderTransformExists();
				renderTransformRotationScale.localPosition = new UnityEngine.Vector3(value.X, -value.Y, -value.Z);
			}
		}

		private global::Astrolabe.Twinkle.Vector3 _translationAfterRotation = global::Astrolabe.Twinkle.Vector3.Zero;

		/// <summary>
		/// Le Component est up mais le VisualElement n'est pas encore disponible
		/// </summary>
		private void Awake()
		{
			// Dans le cas ou l'on souhaite que les children soient stocké dans un GO particulier
			//this.childrenContainer = this.transform.Find("ContentPresenter");

#if UNITY_EDITOR
			try
			{
				_pixelConverter = TwinkleApplication.Instance.Framework.Settings.PixelConverter;
			}
			catch (NullReferenceException)
			{
				// cas particulier et specifique à l'éditeur
				// il est possible qu'un VisualElement soit ajouté à la racine de la scene dynamiquement lorsque l'editeur crash lors d'une recompilation ou d'un arrêt de l'application par le user
				// On va donc le détruire nous même sinon il démarrera peut être avant que AstrolabeManager soit chargé
				var root = gameObject.GetRoot();

				Log.WriteLine(
					"The VisualElement " + root.name +
					" has been removed by Astrolabe cause it is an artefact of the editor. Please remove it from the scene!",
					LogMessageType.Warning);

				Destroy(root);
				return;
			}
#else
            _pixelConverter = TwinkleApplication.Instance.Framework.Settings.PixelConverter;
#endif

			AwakeOverride();
		}

		/// <summary>
		/// à partir du Initialize le Awake est passé et le VisualElement est disponible
		/// </summary>
		private void Initialize()
		{
			SetWidth(Width);
			SetHeight(Height);
			SetForward(Forward);

			InitializeOverride();

			var logicalElement = _visualElement.LogicalElement;

			if (logicalElement != null)
			{
				// ici le VisualElement est une fenetre
				if (_visualElement.IsLogicalWindow)
				{
					// AstrolabeInput qui gère les entrée MRTK est disponible sur toute les fenetres désormais et plus sur AstrolabeManager
					_visualElement.TwinkleComponent.gameObject.AddComponent<AstrolabeInput>();
				}
			}
		}

		public virtual void InitializeOverride()
		{
		}

		protected abstract void AwakeOverride();

		private void Start()
		{
			//Les éléments 2D doivent être sur le layer UI
			is2DElement = ((LogicalElement)_visualElement?.LogicalElement)?.GetWindow() is LogicalWindow2D;

			if (is2DElement)
			{
				gameObject.SetLayerRecursively(5);
			}
		}

		// Renvoi l'endroit ou les enfants sont ajoutées
		// on peut fixer childrenContainer pour avoir un endroit different de rendertransform ou transform comme parent

		public virtual Transform GetChildrenContainer()
		{
			if (_childrenContainer == null)
			{
				if (renderTransformOrigin != null)
				{
					return renderTransformOrigin;
				}

				return transform;
			}

			return _childrenContainer;
		}

		public virtual Transform GetRenderTransform()
		{
			return renderTransform;
		}

		public void AddChild(IVisualElement visualElement)
		{
			var child = visualElement.Element as TwinkleComponent;
			var childrenContainer = GetChildrenContainer();

			// On est en mode world
			child.transform.SetParent(childrenContainer, visualElement.LogicalElement is LogicalWindow);
		}

		public void RemoveChild(IVisualElement visualElement)
		{
			var child = visualElement.Element as TwinkleComponent;

			if (visualElement.LogicalElement is LogicalWindow)
			{
				// On est en mode world
				child.transform.SetParent(null, true);
			}
			else
			{
				child.transform.SetParent(null, false);
			}
		}
	}
}