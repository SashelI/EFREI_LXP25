using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.IO;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = Astrolabe.Twinkle.Vector2;
//using UnityEngine.Rendering.Universal;

namespace Assets.Astrolabe.Scripts.Components
{
	/// <summary>
	/// Window utilisée pour faire de l'UI 2D (Unity Canvas, hardware type tablette)
	/// </summary>
	[TwinkleLogicalObject("Window2D")]
	public class LogicalWindow2D : LogicalWindow
	{
		/// <summary>
		/// Sets where to anchor the canvas on the screen (left, right, center, upper, down)
		/// </summary>
		[TwinkleLogicalProperty]
		public AnchoredSide CanvasAnchor
		{
			get => _canvasAnchor;

			set
			{
				//En unity, bas à gauche du canvas = coordonnée 0,0.
				var anchorMinMax = value switch
				{
					AnchoredSide.UpperLeft => new UnityEngine.Vector2(0, 1),
					AnchoredSide.UpperRight => new UnityEngine.Vector2(1, 1),
					AnchoredSide.UpperCenter => new UnityEngine.Vector2(0.5f, 1),
					AnchoredSide.LowerLeft => UnityEngine.Vector2.zero,
					AnchoredSide.LowerRight => new UnityEngine.Vector2(1, 0),
					AnchoredSide.LowerCenter => new UnityEngine.Vector2(0.5f, 0),
					AnchoredSide.CenterLeft => new UnityEngine.Vector2(0, 0.5f),
					AnchoredSide.CenterRight => new UnityEngine.Vector2(1, 0.5f),
					_ => new UnityEngine.Vector2(0.5f, 0.5f)
				};

				if (_rectTransform != null)
				{
					_rectTransform.anchorMin = _rectTransform.anchorMax = anchorMinMax;
				}

				_anchorMinMax = anchorMinMax.ToVector2();
				_canvasAnchor = value;
			}
		}
		private AnchoredSide _canvasAnchor = AnchoredSide.Center;

		private Vector2 _anchorMinMax = new(0.5f, 0.5f);

		/// <summary>
		/// Sets the default device orientation for the app (will determine how the canvas scales)
		/// </summary>
		[TwinkleLogicalProperty]
		public AppOrientation AppOrientation
		{
			get => _orientation;

			set
			{
				if (_scaler != null)
				{
					_scaler.matchWidthOrHeight = value switch
					{
						AppOrientation.Portrait => 1.0f,
						AppOrientation.Landscape => 0.0f,
						_ => 0.5f
					};

					if (_referenceResolution == Vector2.Zero)
					{
						_referenceResolution = value switch
						{
							AppOrientation.Portrait => new Vector2(1080, 1920),
							_ => new Vector2(1920, 1080)
						};
					}
				}
				_orientation = value;
			}
		}
		private AppOrientation _orientation = AppOrientation.All;

		/// <summary>
		/// The resolution to use as a reference for editing
		/// </summary>
		[TwinkleLogicalProperty]
		public Vector2 ReferenceResolution
		{
			get => _referenceResolution;

			set
			{
				if (_scaler != null)
				{
					_scaler.referenceResolution = value.ToVector2();
				}
				_referenceResolution = value;
			}
		}
		private Vector2 _referenceResolution = Vector2.Zero;

		[TwinkleLogicalProperty]
		public bool SecondaryRenderingCamera
		{
			get => _secondaryRenderingCamera;
			set
			{
				Camera secondaryCamera = null;

				foreach (Transform secondCam in Camera.main.transform)
				{
					secondCam.TryGetComponent(out secondaryCamera);
				}

				if (value)
				{
					if (secondaryCamera == null)
					{
						var camGo = new GameObject("UI Cam");
						secondaryCamera = camGo.AddComponent<Camera>();
						//secondaryCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
						secondaryCamera.cullingMask = 1 << 5;
						camGo.transform.SetParent(Camera.main.transform);
					}

					_canvas.worldCamera = secondaryCamera;
					//Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(secondaryCamera);
				}
				else
				{
					_canvas.worldCamera = Camera.main;
					//Camera.main.GetUniversalAdditionalCameraData().cameraStack.Remove(secondaryCamera);
				}

				_secondaryRenderingCamera = value;
			}
		}
		private bool _secondaryRenderingCamera = false;

		private Canvas _canvas;
		private RectTransform _rectTransform;
		private CanvasScaler _scaler;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.InitializeComponent();
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			this.GetGameObject().SetLayerRecursively(5);

			if (!this.GetGameObject().TryGetComponent(out _rectTransform))
			{
				_rectTransform = this.GetGameObject().AddComponent<RectTransform>();
			}

			_rectTransform.anchorMax = _rectTransform.anchorMin = _anchorMinMax.ToVector2();

			if (!this.GetGameObject().TryGetComponent(out _canvas))
			{
				_canvas = this.GetGameObject().AddComponent<Canvas>();
			}

			if (!this.GetGameObject().TryGetComponent(out GraphicRaycaster caster))
			{
				caster = this.GetGameObject().AddComponent<GraphicRaycaster>();
			}

			_canvas.renderMode = RenderMode.ScreenSpaceCamera; //Si l'on met "overlay", les éléments 3D ne seront pas rendus
			_canvas.worldCamera = Camera.main;

			if (!this.GetGameObject().TryGetComponent(out _scaler))
			{
				_scaler = this.GetGameObject().AddComponent<CanvasScaler>();
			}

			_scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			_scaler.matchWidthOrHeight = _orientation switch //En portrait, on match la height pour scale. En paysage, on match la width.
			{
				AppOrientation.Portrait => 1.0f,
				AppOrientation.Landscape => 0.0f,
				_ => 0.5f
			};
			_scaler.referenceResolution = _referenceResolution.ToVector2();
		}
	}

	public enum AppOrientation
	{
		Portrait,
		Landscape,
		All
	}

	public enum AnchoredSide
	{
		UpperLeft,
		UpperRight,
		UpperCenter,
		LowerLeft,
		LowerRight,
		LowerCenter,
		CenterLeft,
		CenterRight,
		Center
	}
}