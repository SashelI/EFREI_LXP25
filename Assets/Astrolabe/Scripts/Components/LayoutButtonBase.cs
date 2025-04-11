using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.UX;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutButtonBase : TwinkleComponent
	{
		#region CONST

		private const string BASE_COLOR = "_Base_Color_";
		private const string EDGE_COLOR = "_Edge_Color_";
		private const string LINE_WIDTH = "_Line_Width_";
		private const string COLOR = "_Color_";
		private const string INNER_COLOR = "_Inner_Color_";
		private const string RADIUS = "_Radius_";
		private const string BEVEL_RADIUS = "_Bevel_Radius_";

		private const string COMPRESSABLE_VISUALS_NAME = "CompressableButtonVisuals";
		private const string FRONTPLATE_NAME = "FrontPlate";
		private const string FRONTPLATE_HIGHLIGHT_NAME = "UX.Button.FrontplateHighlight";
		private const string BACKPLATE_NAME = "ButtonContent";
		private const string BACKPLATE_HIGHLIGHT_NAME = "UX.Button.BackplateHighlight";

		#endregion

		private Transform _buttonContainer;

		private Transform _compressableButtonVisuals;
		private Transform _switchToggleVisuals;
		private BoxCollider _boxCollider;

		private PressableButton _pressableButton;

		//Renderers pour le highlight de la backplate et le hover de la front plate (le highlight et le hover border sont gérés dans le matériau)
		private MeshRenderer _frontPlateHighlight;
		private MeshRenderer _backPlateHighlight;

		protected override void SetWidthOverride(float width)
		{
			if (_frontPlateHighlight != null)
			{
				var frontPlateScale = _frontPlateHighlight.transform.parent.localScale;
				_frontPlateHighlight.transform.parent.localScale = new Vector3(width - 0.07f * width, frontPlateScale.y, frontPlateScale.z);
			}

			if (_backPlateHighlight != null)
			{
				var backPlateScale = _backPlateHighlight.transform.parent.localScale;
				_backPlateHighlight.transform.parent.localScale = new Vector3(width - 0.07f * width, backPlateScale.y, backPlateScale.z);
			}

			var size = _boxCollider.size;
			_boxCollider.size = new Vector3(width, size.y, size.z);

			if (_switchToggleVisuals != null)
			{
				_switchToggleVisuals.localScale = Vector3.one + new Vector3(width * 20, width * 20, width * 20);
			}
		}

		protected override void SetHeightOverride(float height)
		{
			if (_frontPlateHighlight != null)
			{
				var frontPlateScale = _frontPlateHighlight.transform.parent.localScale;
				_frontPlateHighlight.transform.parent.localScale = new Vector3(frontPlateScale.x, height - 0.07f * height, frontPlateScale.z);
			}

			if (_backPlateHighlight != null)
			{
				var backPlateScale = _backPlateHighlight.transform.parent.localScale;
				_backPlateHighlight.transform.parent.localScale = new Vector3(backPlateScale.x, height - 0.07f * height, backPlateScale.z);
			}

			var size = _boxCollider.size;
			_boxCollider.size = new Vector3(size.x, height, size.z);
		}

		protected override void SetDepthOverride(float depth)
		{
			var halfDepth = depth / 2;

			var size = _boxCollider.size;
			_boxCollider.size = new Vector3(size.x, size.y, depth);

			var localPosition = _buttonContainer.localPosition;
			_buttonContainer.localPosition = new Vector3(localPosition.x, localPosition.y, -halfDepth);
		}

		protected override void AwakeOverride()
		{
			_buttonContainer = transform.GetChild(0);

			_boxCollider = _buttonContainer.GetComponent<BoxCollider>();
			_pressableButton = _buttonContainer.GetComponent<PressableButton>();

			_compressableButtonVisuals = _buttonContainer?.Find(COMPRESSABLE_VISUALS_NAME);

			var frontPlate = _compressableButtonVisuals?.Find(FRONTPLATE_NAME);
			_frontPlateHighlight = frontPlate?.Find(FRONTPLATE_HIGHLIGHT_NAME)?.GetComponent<MeshRenderer>();

			var backPlate = _buttonContainer?.Find(BACKPLATE_NAME)?.GetChild(0);
			_backPlateHighlight = backPlate?.Find(BACKPLATE_HIGHLIGHT_NAME)?.GetComponent<MeshRenderer>();
		}

		public bool BorderOnHover
		{
			get => _frontPlateHighlight.enabled;

			set => _frontPlateHighlight.enabled = value;
		}

		public float HoverBorderForward
		{
			get => _hoverBorderForward;

			set
			{
				if (_hoverBorderForward != value)
				{
					var pos = _frontPlateHighlight.transform.position;
					_frontPlateHighlight.transform.position = new Vector3(pos.x, pos.y, value);
					_hoverBorderForward = value;
				}
			}
		}

		private float _hoverBorderForward = 1f;

		public Color HoverBorderColor
		{
			get => _frontPlateHighlight.material.GetColor(EDGE_COLOR);
			set => _frontPlateHighlight.material.SetColor(EDGE_COLOR, value);
		}

		public float HoverBorderThickness
		{
			get => _frontPlateHighlight.material.GetFloat(LINE_WIDTH);
			set => _frontPlateHighlight.material.SetFloat(LINE_WIDTH, value);
		}

		public bool HighlightOnSelect
		{
			get => _backPlateHighlight.enabled;
			set => _backPlateHighlight.enabled = value;
		}

		public Color OnSelectHighlightColor
		{
			get => _backPlateHighlight.material.GetColor(COLOR);
			set
			{
				_backPlateHighlight.material.SetColor(COLOR, value);

				var innerColorR = Mathf.Clamp(value.r * 255 - 83, 0, 255) / 255f;
				var innerColorG = Mathf.Clamp(value.g * 255 - 78, 0, 255) / 255f;
				var innerColorB = Mathf.Clamp(value.b * 255 - 52, 0, 255) / 255f;

				var innerColor = new Color(innerColorR, innerColorG, innerColorB, value.a);

				_backPlateHighlight.material.SetColor(INNER_COLOR, innerColor);
			}
		}

		public float OnSelectHighlightThickness
		{
			get => _backPlateHighlight.material.GetFloat(LINE_WIDTH);
			set => _backPlateHighlight.material.SetFloat(LINE_WIDTH, value);
		}

		public CornerRadius CornerRadius
		{
			get
			{
				var radius = _frontPlateHighlight.material.GetFloat(RADIUS);
				return new CornerRadius(radius);
			}
			set
			{
				var maxRadiusBottom = Mathf.Max(value.BottomLeft, value.BottomRight);
				var maxRadiusTop = Mathf.Max(value.TopLeft, value.TopRight);
				var maxRadius = Mathf.Max(maxRadiusTop, maxRadiusBottom);

				//On set le radius des highlight au max des radius du rectangle :
				//le shader n'accepte qu'une valeur uniforme donc on prend le coin le plus arrondi
				_frontPlateHighlight.material.SetFloat(RADIUS, maxRadius.ToVisual());
				_backPlateHighlight.material.SetFloat(BEVEL_RADIUS,
					maxRadius.ToVisual() + 0.008f); //petit offset pour des raisons de shader
			}
		}

		public Color SwitchToggleColor
		{
			get
			{
				if (_switchToggleVisuals != null)
				{
					if (_switchToggleVisuals.TryGetComponent(out AstrolabeSwitchToggleVisuals visuals))
					{
						return visuals.backplateMesh.material.GetColor(BASE_COLOR);
					}
				}
				return default;
			}
			set
			{
				if (_switchToggleVisuals != null)
				{
					if (_switchToggleVisuals.TryGetComponent(out AstrolabeSwitchToggleVisuals visuals))
					{
						visuals.backplateMesh.material.SetColor(BASE_COLOR, value);
					}
				}
			}
		}


		public bool IsGrabEnabled
		{
			get
			{
				if (_pressableButton == null)
				{
					return false;
				}

				GrabInteractor grab = new();
				return _pressableButton.IsInteractorTypeValid(grab) && _pressableButton.enabled;
			}

			set
			{
				if (_pressableButton != null)
				{
					if (!_pressableButton.enabled && value)
					{
						_pressableButton.enabled = true;
					}

					if (value)
					{
						_pressableButton.EnableInteractorType(typeof(IGrabInteractor));
					}
					else
					{
						_pressableButton.DisableInteractorType(typeof(IGrabInteractor));
					}
				}
			}
		}

		/// <summary>
		/// Adds a child prefab to the button container (used for SwitchToggle)
		/// </summary>
		/// <param name="child"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		public Transform AddChildToContainer(Transform child, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (!IsChildInContainer(child.name, out _)) //We only add the child if not already there
			{
				var instance = Instantiate(child, position, rotation, _buttonContainer);
				var width = _boxCollider.size.x; //(cf SetWidthOverride)
				instance.localScale = scale + new Vector3(width * 20, width * 20, width * 20);
				_switchToggleVisuals = instance;
				return instance;
			}

			return null;
		}

		public void RemoveChildFromContainer(string childName)
		{
			if (IsChildInContainer(childName, out var child))
			{
				if (_switchToggleVisuals.name.Equals(childName))
				{
					_switchToggleVisuals = null;
				}
				Destroy(child);
			}
		}

		private bool IsChildInContainer(string childName, out Transform childTransform)
		{
			childTransform = null;
			foreach (Transform child in _buttonContainer)
			{
				if (child.name.Equals(childName))
				{
					childTransform = child;
					return true;
				}
			}
			return false;
		}

		public void GetPressableButtonComponent(out PressableButton pressableButton)
		{
			pressableButton = _pressableButton;
		}
	}
}