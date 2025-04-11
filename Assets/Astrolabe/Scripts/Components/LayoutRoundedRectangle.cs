using System;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Components
{
	public class LayoutRoundedRectangle : TwinkleComponent
	{
		private static readonly int _BorderColorId;
		private static readonly int _borderWidthId;
		private static readonly int _roundCornersRadiusId;
		private static readonly int _borderLightOpaqueAlphaId;

		private float _opacity = 1;

		static LayoutRoundedRectangle()
		{
			_BorderColorId = Shader.PropertyToID("_BorderColor");
			_borderWidthId = Shader.PropertyToID("_BorderWidth");
			_roundCornersRadiusId = Shader.PropertyToID("_RoundCornersRadius");
			_borderLightOpaqueAlphaId = Shader.PropertyToID("_BorderLightOpaqueAlpha");
		}

		private MaterialPropertyBlock _materialPropertyBlock;
		private MeshRenderer _quadMeshRenderer;
		private Transform _quad;

		private Transform _quadBorder;
		private MeshRenderer _meshRendererBorder;
		private MaterialPropertyBlock _materialPropertyBlockBorder;

		private SolidColorBrush _borderBrush;
		private float _borderThickness;

		private RoundedQuadMesh _quadRoundedQuadMeshComponent;
		private BoxCollider _quadCollider;

		private FluentShader _fluentShader = new();

		public Transform RoundedQuad => _quad;

		protected override void SetWidthOverride(float width)
		{
			if (float.IsNaN(Height) == false)
			{
				_quadRoundedQuadMeshComponent.ChangeDimension(width, Height);
				_quadCollider.size = new UnityEngine.Vector3(width, Height, 0.001f);

				ChangeBorderDimension(width, Height);
				// ChangeBorderProperties est appelé dedans
				SetCornerRadius(_cornerRadius);
			}

			_fluentShader.Width = width;
		}

		protected override void SetHeightOverride(float height)
		{
			if (float.IsNaN(Width) == false)
			{
				_quadRoundedQuadMeshComponent.ChangeDimension(Width, height);
				_quadCollider.size = new UnityEngine.Vector3(Width, height, 0.001f);

				ChangeBorderDimension(Width, height);
				// ChangeBorderProperties est appelé dedans
				SetCornerRadius(_cornerRadius);
			}

			_fluentShader.Height = height;
		}

		protected override void SetDepthOverride(float depth)
		{
			base.SetDepthOverride(depth);

			_fluentShader.Depth = depth;
		}

		private void ChangeBorderDimension(float width, float height)
		{
			var scale = _quadBorder.localScale;
			_quadBorder.localScale = new UnityEngine.Vector3(width, height, scale.z);
		}

		/// <summary>
		/// CornerRadius
		/// </summary>
		public CornerRadius CornerRadius
		{
			get => _cornerRadius;

			set
			{
				if (_cornerRadius != value)
				{
					SetCornerRadius(value);
				}
			}
		}

		private CornerRadius _cornerRadius;

		private void SetCornerRadius(CornerRadius value)
		{
			_cornerRadius = value;

			if (float.IsNaN(Width) == false && float.IsNaN(Height) == false)
			{
				if (value != CornerRadius.Empty)
				{
					var ray = Math.Min(Width, Height) / 2;

					var topLeft = Mathf.Clamp(value.TopLeft / ray, 0f, 1f);
					var topRight = Mathf.Clamp(value.TopRight / ray, 0f, 1f);
					var bottomLeft = Mathf.Clamp(value.BottomLeft / ray, 0f, 1f);
					var bottomRight = Mathf.Clamp(value.BottomRight / ray, 0f, 1f);

					// Le component QuadMesh prends les corners dans un sens different de celui de XAML
					_quadRoundedQuadMeshComponent.ChangeCornerRadius(topLeft, topRight, bottomLeft, bottomRight);
				}
				else
				{
					_quadRoundedQuadMeshComponent.ChangeCornerRadius(0, 0, 0, 0);
				}

				// change le CornerRadius
				ChangeBorderProperties(_borderBrush, _borderThickness, _opacity);
			}
		}

		internal void SetBorderForward(float value)
		{
			var forward =
				(value > 0 ? value : 0) + 0.0001f; // On le fait avancer un tout petit peu par rapport à RoundedRectangle

			var position = _quadBorder.localPosition;
			_quadBorder.localPosition = new UnityEngine.Vector3(position.x, position.y, -forward);
		}

		private Material _currentBorderMaterial;

		public void ChangeBorderProperties(SolidColorBrush borderBrush, float borderThickness, float opacity)
		{
			_borderBrush = borderBrush;
			_borderThickness = borderThickness;
			_opacity = Mathf.Clamp(opacity, 0f, 1f);

			if (borderBrush != null && borderThickness > 0)
			{
				_quadBorder.gameObject.SetActive(true);

				// Material par Transparence
				if (borderBrush.Color.IsOpaque == false || opacity < 1)
				{
					_currentBorderMaterial = TwinkleMaterialFactory.Instance.materialT1Border;
				}
				else
				{
					_currentBorderMaterial = TwinkleMaterialFactory.Instance.materialT0Border;
				}

				if (_currentBorderMaterial != _meshRendererBorder.sharedMaterial)
				{
					_meshRendererBorder.sharedMaterial = _currentBorderMaterial;
					_meshRendererBorder.GetPropertyBlock(_materialPropertyBlockBorder);
				}

				// pas de prise en compte de la transparence par emissive color mais par alpha
				_materialPropertyBlockBorder.SetColor(_BorderColorId, borderBrush.Color.ToUnityColor());

				var borderSize = borderThickness;
				_materialPropertyBlockBorder.SetFloat(_borderWidthId, borderSize);

				var cr = _cornerRadius;
				_materialPropertyBlockBorder.SetVector(_roundCornersRadiusId,
					new Vector4(cr.TopLeft, cr.TopRight, cr.BottomLeft, cr.BottomRight));

				// alpha + opacity du border

				var alpha = (float)_borderBrush.Color.A / 255f * _opacity;
				_materialPropertyBlockBorder.SetFloat(_borderLightOpaqueAlphaId, alpha);

				_meshRendererBorder.SetPropertyBlock(_materialPropertyBlockBorder);
			}
			else
			{
				_quadBorder?.gameObject?.SetActive(false);
			}
		}

		// Start is called before the first frame update
		protected override void AwakeOverride()
		{
			_quad = transform.Find("RoundedQuad");

			if (_quad == null)
			{
				_quad = transform.FindRecursively("RoundedQuad");
			}

			_quadRoundedQuadMeshComponent = _quad.GetComponent<RoundedQuadMesh>();

			_quadMeshRenderer = _quad.GetComponent<MeshRenderer>();

			_quadCollider = _quad.GetComponent<BoxCollider>();

			SetCornerRadius(_cornerRadius);

			// border
			_quadBorder = transform.FindRecursively("BorderQuad");
			_meshRendererBorder = _quadBorder.GetComponent<MeshRenderer>();
			_materialPropertyBlockBorder = new MaterialPropertyBlock();

			SetBorderForward(0);

			// fluentelement (Background)
			_materialPropertyBlock = new MaterialPropertyBlock();

			_fluentShader.Initialize(this,
				_materialPropertyBlock,
				_quadMeshRenderer,
				_quadCollider);
		}

		public FluentShader FluentShader => _fluentShader;

		private void OnDestroy()
		{
			if (_fluentShader != null)
			{
				_fluentShader.RemoveThisObjectFromCache();
				var textureToDestroy = _fluentShader.MediaTexture;
				_fluentShader.Dispose();
				_fluentShader = null;

				if (textureToDestroy != null &&
				    textureToDestroy.name != "UnityWhite") //On ne veut (peut) pas supprimer une texture par défaut
				{
					Destroy(textureToDestroy);
				}
			}
		}
	}
}