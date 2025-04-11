using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Astrolabe.Twinkle.Tools;
using MixedReality.Toolkit;
using UnityEngine;
using Color = Astrolabe.Twinkle.Color;
using Object = UnityEngine.Object;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualButtonBase : VisualElement, IVisualButtonBase
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalButtonBase), typeof(VisualButtonBase));
		}

		public VisualButtonBase(ILogicalElement logicalElement) : base(logicalElement)
		{
			var buttonBase = Object.Instantiate(TwinklePrefabFactory.Instance.prefabButtonBase);
			_button = buttonBase.GetComponent<LayoutButtonBase>();
			TwinkleComponent = _button;

			var layoutButtonBaseEvents = buttonBase.GetChild(0).GetComponent<LayoutButtonBaseEvents>();
			layoutButtonBaseEvents.VisualElement = this;
		}

		private readonly LayoutButtonBase _button;

		public bool BorderOnHover
		{
			get => _button.BorderOnHover;

			set => _button.BorderOnHover = value;
		}

		public float HoverBorderForward
		{
			get => _button.HoverBorderForward;
			set => _button.HoverBorderForward = value;
		}

		public Color HoverBorderColor
		{
			get
			{
				var color = _button.HoverBorderColor;
				return new Color((byte)(color.a * 255), (byte)(color.r * 255), (byte)(color.g * 255),
					(byte)(color.b * 255));
			}
			set => _button.HoverBorderColor = value.ToUnityColor();
		}

		public float HoverBorderThickness
		{
			get => _button.HoverBorderThickness.ToLogical();
			set => _button.HoverBorderThickness = value.ToVisual();
		}

		public bool HighlightOnSelect
		{
			get => _button.HighlightOnSelect;
			set => _button.HighlightOnSelect = value;
		}

		public Color OnSelectHighlightColor
		{
			get
			{
				var color = _button.OnSelectHighlightColor;
				return new Color((byte)(color.a * 255), (byte)(color.r * 255), (byte)(color.g * 255),
					(byte)(color.b * 255));
			}
			set => _button.OnSelectHighlightColor = value.ToUnityColor();
		}

		public float OnSelectHighlightThickness
		{
			get => _button.OnSelectHighlightThickness.ToLogical();
			set => _button.OnSelectHighlightThickness = value.ToVisual();
		}

		public bool IsGrabEnabled
		{
			get => _button.IsGrabEnabled;

			set => _button.IsGrabEnabled = value;
		}

		public CornerRadius CornerRadius
		{
			get => _button.CornerRadius;
			set => _button.CornerRadius = value;
		}

		public Color SwitchToggleColor
		{
			get
			{
				var color = _button.SwitchToggleColor;
				return new Color((byte)(color.a * 255), (byte)(color.r * 255), (byte)(color.g * 255),
					(byte)(color.b * 255));
			}
			set => _button.SwitchToggleColor = value.ToUnityColor();
		}

		public bool IsSwitchButton
		{
			get => _isSwitchButton;
			set
			{
				if (_isToggleButton && value != _isSwitchButton)
				{
					if (value)
					{
						var switchToggleVisuals = _button.AddChildToContainer(TwinklePrefabFactory.Instance.prefabSwitchToggle, UnityEngine.Vector3.zero, Quaternion.identity, UnityEngine.Vector3.one);
						_button.GetPressableButtonComponent(out var pressable);

						if (switchToggleVisuals != null && pressable != null)
						{
							if (switchToggleVisuals.TryGetComponent(out AstrolabeSwitchToggleVisuals visuals))
							{
								visuals.statefulInteractable = pressable;
							}
						}
					}
					else
					{
						_button.RemoveChildFromContainer(TwinklePrefabFactory.Instance.prefabSwitchToggle.name);
					}
				}

				_isSwitchButton = value;
			}
		}
		private bool _isSwitchButton = false;

		public bool IsToggleButton
		{
			get => _isToggleButton;
			set
			{
				SetPressableToToggleOrSelect(value);
				_isToggleButton = value;
			}
		}
		private bool _isToggleButton;

		private void SetPressableToToggleOrSelect(bool isToggle)
		{
			_button.GetPressableButtonComponent(out var pressable);

			if (pressable != null)
			{
				pressable.ToggleMode = isToggle ? StatefulInteractable.ToggleType.Toggle : StatefulInteractable.ToggleType.Button;
			}
		}
	}
}