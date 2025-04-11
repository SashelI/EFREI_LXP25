using System;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Visuals;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public class FluentShader
	{
		private static readonly int _mainTex;
		private static readonly int _fluentLightIntensity;
		private static readonly int _hoverColorOverride;
		private static readonly int _proximityLightCenterColorOverride;
		private static readonly int _proximityLightMiddleColorOverride;
		private static readonly int _proximityLightOuterColorOverride;
		private static readonly int _color;
		private static readonly int _mainTexSt;
		private static readonly int _cull;

		static FluentShader()
		{
			_mainTex = Shader.PropertyToID("_MainTex");
			_fluentLightIntensity = Shader.PropertyToID("_FluentLightIntensity");

			_hoverColorOverride = Shader.PropertyToID("_HoverColorOverride");
			_proximityLightCenterColorOverride = Shader.PropertyToID("_ProximityLightCenterColorOverride");
			_proximityLightMiddleColorOverride = Shader.PropertyToID("_ProximityLightMiddleColorOverride");
			_proximityLightOuterColorOverride = Shader.PropertyToID("_ProximityLightOuterColorOverride");

			_color = Shader.PropertyToID("_Color");
			_mainTexSt = Shader.PropertyToID("_MainTex_ST");
			_cull = Shader.PropertyToID("_Cull");
		}

		private MaterialPropertyBlock _materialPropertyBlock;

		private const float MAX_TILING_SCALE = 100000000;

		private TwinkleComponent _rootComponent;
		private MeshRenderer _meshRenderer; //-> anciennement quadRender
		private Material _currentMaterial; // -> anciennement quadMaterial

		private ImageSource _imageSource = null;

		private bool _isMaterialFromPool = true;

		private bool _isTransparencyEnabled = false;
		private bool _isRoundCornerEnabled = false;
		private bool _isLightEnabled;

		private SolidColorBrush _lightColor;

		private static Texture2D EmptyTexture
		{
			get
			{
				if (_emptyTexture == null)
				{
					_emptyTexture = TwinkleMaterialFactory.Instance.emptyTexture;
				}

				return _emptyTexture;
			}
		}

		private static Texture2D _emptyTexture;

		public Texture MediaTexture
		{
			get => _mediaTexture;
			set => _mediaTexture = value;
		}

		private Texture _mediaTexture;

		// couleur du rectangle et de la texture  (qui a besoin d'être blanche)
		private UnityEngine.Color _colorWithoutOpacity = new(1f, 1f, 1f, 1f);

		public void Initialize(TwinkleComponent rootComponent, MaterialPropertyBlock materialPropertyBlock,
			MeshRenderer meshRenderer, Collider collider)
		{
			_rootComponent = rootComponent;
			_meshRenderer = meshRenderer;
			_collider = collider;
			_materialPropertyBlock = materialPropertyBlock;

			// le material par defaut est toujours le opaque (nous l'avons defini ainsi)

			// Opacity, Color, Texture, Tiling est géré par le Brush
			SetBackground(null, null);

			_isLightEnabled = TwinkleApplication.Instance.Framework.Settings.LightEnabled;
			_lightColor = TwinkleApplication.Instance.Framework.Settings.DefaultLightColor;

			// Le SetTexture est à null + isTransprency = false + isLightEnabled = à true
			ChangeMaterial(true);
		}

		public Brush Brush => _currentBrush;

		private Brush _currentBrush;

		/// <summary>
		/// Quand on change de material on va chercher le material et lui applique toutes les propriétés seulement si le material change
		/// </summary>
		/// <returns></returns>
		private bool ChangeMaterial(bool forceApplyMaterialPropertyBlocks)
		{
			var isTransparencyEnabled = _isTransparencyEnabled;

			if (BrushType == BrushTypes.Texture)
			{
				//if(this.currentBrush.GetType() != typeof(VideoBrush))
				//{
				// C'est une texture
				_isTransparencyEnabled = true;
				//}
				//else
				//{
				//    isTranspencyEnabled = false;
				//}
			}

			Material material;

			// Le Brush n'est pas vide (c'est peut être une instance NEW)
			if (_currentBrush != null)
			{
				if (_currentBrush.InstanceMethod == InstanceMethods.New)
				{
					// On demande la creation ou recuperation d'un Material si InstanceMethod == New
					_isMaterialFromPool = false;
					material = TwinkleMaterialFactory.Instance.CreateOrGetNewInstanceMaterial(_currentBrush,
						_isTransparencyEnabled, _shaderMode);
				}
				else
				{
					// On demande la recuperation d'un Material partagé si InstanceMethod == New
					_isMaterialFromPool = true;
					material = TwinkleMaterialFactory.Instance.GetMaterialFromPool(_isTransparencyEnabled,
						_isRoundCornerEnabled, _isLightEnabled, _shaderMode);
				}
			}
			else
			{
				_isMaterialFromPool = true;
				material = TwinkleMaterialFactory.Instance.GetMaterialFromPool(_isTransparencyEnabled, _isRoundCornerEnabled,
					_isLightEnabled, _shaderMode);
			}

			if (_meshRenderer != null && _meshRenderer.sharedMaterial != material)
			{
				_currentMaterial = material;

				if (_isMaterialFromPool == false)
				{
					// retire le setPropertyBlock s'il existe
					_meshRenderer.SetPropertyBlock(null);
				}

				_meshRenderer.sharedMaterial = material;

				// Uniquement pour InstanceMethods.New
				InitializeKeyWordsMaterial();
				InitializePropertiesMaterial();

				return true;
			}
			else
			{
				if (forceApplyMaterialPropertyBlocks)
				{
					// l'un ou
					SetPropertyBlock(_isMaterialFromPool);
					// l'autre (test interne)
					InitializeKeyWordsMaterial();
				}
			}

			return false;
		}

		private void InitializeKeyWordsMaterial()
		{
			if (_isMaterialFromPool)
			{
				return;
			}

			if (_currentMaterial == null)
			{
				return;
			}

			if (_isLightEnabled == true)
			{
				_currentMaterial.EnableKeyword("_PROXIMITY_LIGHT");
				_currentMaterial.EnableKeyword("_HOVER_LIGHT");
			}
			else
			{
				_currentMaterial.DisableKeyword("_PROXIMITY_LIGHT");
				_currentMaterial.DisableKeyword("_HOVER_LIGHT");
			}

			if (_isRoundCornerEnabled)
			{
				_currentMaterial.EnableKeyword("_ROUND_CORNERS");
			}
			else
			{
				_currentMaterial.DisableKeyword("_ROUND_CORNERS");
			}
		}

		public void SetBackground(Brush value, Action<Brush> sizeChanged)
		{
			_haveBackground = false;

			if (value != null)
			{
				// ** On fixe le currentBrush en cours **

				// desabonne aux notification de l'ancien brush et on s'abonne aux changements
				if (_currentBrush != null)
				{
					_currentBrush.ValueChanged -= OnValueBrushChanged;
				}

				// affectation du brush en cours
				_currentBrush = value;

				// abonnement au nouveau brush en cas de changement directement sur le brush (Color, Source, TileScaleX, ...)

				if (value != null)
				{
					value.ValueChanged += OnValueBrushChanged;
				}
				else
				{
					// nettoyage par defaut
				}

				_mediaTexture = null;

				SetBackgroundMaterial(value, sizeChanged);
			}

			RefreshGameObject();
		}

		/// <summary>
		/// Gérer le material
		/// </summary>
		/// <param name="value"></param>
		/// <param name="sizeChanged"></param>
		private void SetBackgroundMaterial(Brush value, Action<Brush> sizeChanged)
		{
			_isMaterialFromPool = true;

			// Opacity et Color appartiennent à TOUS les brushs
			// On les traite en premier

			// permet de se souvenir de la couleur utilisé
			_colorWithoutOpacity = value.Color.ToUnityColor();

			SetLightEnabled(_isLightEnabled, false);
			// appliquera le changement de couleur et le changement de material
			SetOpacity(_opacity, false);

			if (value is SolidColorBrush)
			{
				// Le solidColorBrush equivaut à la classe abstract Brush (Color+Opacity)
				BrushType = BrushTypes.Color;

				SetImageSource(null, false);

				// Le SetTexture est à null
				ChangeMaterial(true);

				// la taille ne change jamais (c'est important car on va faire un Invalidate sur image ou video par exemple)
				//sizeChanged?.Invoke(value);

				_haveBackground = true;
			}
			else if (value is ImageBrush)
			{
				BrushType = BrushTypes.Texture;

				var imageBrush = (ImageBrush)value;

				var imageSource = imageBrush.Source;

				ApplyTextureBrush((TextureBrush)value);

				// haveBackground est fixé dans SetImageSource
				SetImageSource(imageSource, false);

				// On change de material si besoin et on applique à tous les coups les propriétés
				ChangeMaterial(true);

				sizeChanged?.Invoke(value);
			}
			else if (value is ColoredTextureBrush)
			{
				BrushType = BrushTypes.Texture;

				ApplyTextureBrush((ColoredTextureBrush)value);

				// haveBackground est fixé dedans
				SetColoredTexture(false);

				ChangeMaterial(true);

				sizeChanged?.Invoke(value);
			}
			else if (value is VideoBrush)
			{
				BrushType = BrushTypes.Texture;

				var videoBrush = (VideoBrush)value;
				var videoSource = videoBrush.Source;
				VisualMediaPlayerController controller;

				if (videoBrush.VisualMediaPlayerController == null)
				{
					controller = new VisualMediaPlayerController();
					videoBrush.VisualMediaPlayerController = controller;

					EventHandler textureCreated = (object sender, EventArgs e) =>
					{
						var currentVideoBrush = (VideoBrush)_currentBrush;
						var currentController = (VisualMediaPlayerController)currentVideoBrush.VisualMediaPlayerController;
						var videoPlayer = currentController?.VideoPlayer;

						if (videoPlayer.isPrepared)
						{
							var texture = currentController.Texture;

							SetTexture(texture, false);
						}
					};

					EventHandler readyToPlay = (object sender, EventArgs args) =>
					{
						// permet de fixer la taille
						var currentVideoBrush = (VideoBrush)_currentBrush;
						var currentController = (VisualMediaPlayerController)currentVideoBrush.VisualMediaPlayerController;
						var videoPlayer = currentController?.VideoPlayer;

						if (videoPlayer.isPrepared)
						{
							var texture = currentController.Texture;

							if (texture != null)
							{
								texture.wrapMode = _tileMode == TileMode.None
									? TextureWrapMode.Clamp
									: TextureWrapMode.Repeat;

								// on n'utilise currentBrush car l'evenement peut être declenché par SetBackgroundMaterial ou un changement de valeur de la source de brush (via l'event Video_PrepareCompleted)
								var currentSource = currentVideoBrush?.Source;

								if (currentSource != null)
								{
									var hasSizeChanged = false;

									if (currentSource.PixelWidth != texture.width ||
									    currentSource.PixelHeight != texture.height)
									{
										hasSizeChanged = true;
									}

									currentSource.UpdateSize(texture.width, texture.height);

									_haveBackground = true;
									RefreshGameObject();

									if (hasSizeChanged == true)
									{
										sizeChanged?.Invoke(currentVideoBrush);
									}
								}
							}

							ApplyTextureBrush((TextureBrush)value);

							// On change de material si besoin et on applique à tous les coups les propriétés
							ChangeMaterial(true);
						}
					};

					controller.TextureCreated += textureCreated;
					controller.ReadyToPlay += readyToPlay;
				}
				else
				{
					ApplyTextureBrush((TextureBrush)value);
					controller = (VisualMediaPlayerController)videoBrush.VisualMediaPlayerController;
				}

				// haveBackground est fixé dans SetImageSource
				SetVideoSource(controller, videoSource);
			}
			else
			{
				throw new Exception("Unknown type of brush!");
			}
		}

		private void ApplyTextureBrush(TextureBrush textureBrush)
		{
			// Les propriétés de FluentShader (control) sont prioritaire sur celle de ImageBrush/VideoBrush si leurs valeurs sont differentes de celle par défaut
			var tm = _tileMode != TileMode.None ? _tileMode : textureBrush.TileMode;
			SetTileMode(tm, false);

			var to = _tileOffset != global::Astrolabe.Twinkle.Vector2.Zero ? _tileOffset : textureBrush.TileOffset;
			SetTileOffset(to, false);

			var ts = _tileScale != global::Astrolabe.Twinkle.Vector2.One ? _tileScale : textureBrush.TileScale;

			SetTileScale(ts, false);

			var tax = _alignmentX != AlignmentX.Center ? _alignmentX : textureBrush.AlignmentX;
			var tay = _alignmentY != AlignmentY.Center ? _alignmentY : textureBrush.AlignmentY;
			SetAlignment(tax, tay, false);
		}

		private void RefreshGameObject()
		{
			// si aucun brush disponible on desactive le meshRenderer ce qui le rend invisible (mais le collider reste peut être actif)
			RefreshRenderer();
			RefreshCollider();
		}

		private void SetTexture(Texture texture, bool setProperty)
		{
			_mediaTexture = texture;

			if (texture != null)
			{
				if (_isMaterialFromPool)
				{
					// Texture principal

					_materialPropertyBlock.SetTexture(_mainTex, texture);

					if (setProperty)
					{
						_meshRenderer?.SetPropertyBlock(_materialPropertyBlock);
					}
				}
				else
				{
					if (_currentMaterial.mainTexture != texture)
					{
						_currentMaterial.mainTexture = texture;
					}
				}
			}
		}

		private bool
			SetVideoSource(VisualMediaPlayerController controller, VideoSource videoSource)
		{
			_haveBackground = false;
			controller.Source = videoSource?.Source;

			return true;
		}

		/// <summary>
		/// Gestion de l'imageSource (avec chargement et affectation de la texture)
		/// </summary>
		/// <param name="imageSource"></param>
		/// <returns></returns>
		private bool SetImageSource(ImageSource imageSource, bool setProperty)
		{
			if (_imageSource != null)
			{
				_imageSource.PixelsModified -= OnImageSourcePixelsModified;
			}

			_imageSource = imageSource;

			if (imageSource == null)
			{
				return false;
			}
			else
			{
				_imageSource.PixelsModified += OnImageSourcePixelsModified;
			}

			// chargement à partir du fichier ou de la ressource
			var texture = LoadTexture(imageSource);

			if (texture != null)
			{
				_imageSource.IsLoaded = true;

				SetTexture(texture, setProperty);
				// permet l'affichage ou non du MeshRenderer
				_haveBackground = true;
				RefreshGameObject();

				return true;
			}
			else
			{
				//si imageTexture2D etait null auparavant alors pas besoin de lui fixer une texture vide
				if (_mediaTexture != null)
				{
					_imageSource.IsLoaded = false;

					// Vide la tetxure
					SetTexture(EmptyTexture, setProperty);

					// permet l'affichage ou non du MeshRenderer
					_haveBackground = false;
					RefreshGameObject();
				}
			}

			return false;
		}

		/// <summary>
		/// ImageSource.Pixels a été modifié
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnImageSourcePixelsModified(object sender, EventArgs e)
		{
			var text2D = (Texture2D)_mediaTexture;
			text2D.LoadRawTextureData(_imageSource.Pixels);
			text2D.Apply();
		}

		private void SetColoredTexture(bool setProperty)
		{
			var texture = Texture2D.whiteTexture;

			SetTexture(texture, setProperty);
			// permet l'affichage ou non du MeshRenderer
			_haveBackground = true;

			RefreshGameObject();
		}

		private void OnValueBrushChanged(object sender, EventArgs e)
		{
			var notificationEventArgs = (NotifyPropertyEventArgs)e;

			switch (notificationEventArgs.PropertyId)
			{
				case Brush.COLOR_PROPERTY_ID:
					SetColor((global::Astrolabe.Twinkle.Color)notificationEventArgs.Value, true);
					break;

				case Brush.OPACITY_PROPERTY_ID:
					SetOpacity((float)notificationEventArgs.Value, true);
					break;

				case VideoBrush.SOURCE_PROPERTY_ID:

					var brush = (VideoBrush)_currentBrush;
					SetVideoSource((VisualMediaPlayerController)brush.VisualMediaPlayerController,
						(VideoSource)notificationEventArgs.Value);
					ChangeMaterial(true);
					break;

				case ImageBrush.SOURCE_PROPERTY_ID:
					SetImageSource((ImageSource)notificationEventArgs.Value, true);
					// pas besoin de forcer car le SetImageSource a deja ecris dans MaterialPropertyBlock
					ChangeMaterial(false);
					break;

				case TextureBrush.TILEMODE_PROPERTY_ID:
					SetTileMode((TileMode)notificationEventArgs.Value, true);
					// pas besoin de forcer car le SetTileMode a deja ecris dans MaterialPropertyBlock
					ChangeMaterial(true);
					break;

				case TextureBrush.TILEOFFSET_PROPERTY_ID:
					SetTileOffset((global::Astrolabe.Twinkle.Vector2)notificationEventArgs.Value, true);
					break;

				case TextureBrush.TILESCALE_PROPERTY_ID:
					SetTileScale((global::Astrolabe.Twinkle.Vector2)notificationEventArgs.Value, true);
					break;

				case TextureBrush.ALIGNMENTX_PROPERTY_ID:
					SetAlignment((AlignmentX)notificationEventArgs.Value, _alignmentY, true);
					break;

				case TextureBrush.ALIGNMENTY_PROPERTY_ID:
					SetAlignment(_alignmentX, (AlignmentY)notificationEventArgs.Value, true);
					break;
			}
		}

		public void SetLightIntensity(float lightIntensity, bool setProperty = true)
		{
			if (_isMaterialFromPool)
			{
				_materialPropertyBlock.SetFloat(_fluentLightIntensity, lightIntensity);

				if (setProperty == true)
				{
					_meshRenderer?.SetPropertyBlock(_materialPropertyBlock);
				}
			}

			//this.meshRenderer.material.SetFloat("_FluentLightIntensity", lightIntensity);
		}

		public void SetLightEnabled(bool isLightEnabled, bool changeMaterial)
		{
			_isLightEnabled = isLightEnabled;

			if (changeMaterial)
			{
				// pas besoin de forcer le MaterialPropertyBlock ici car IsLightEnabled n'a pas de property à lui (C'est juste un EnabledKeyword)
				ChangeMaterial(false);
			}
		}

		public void SetLightColor(SolidColorBrush color, bool setProperty)
		{
			_lightColor = color;

			var unityColor = color.ToUnityColor();

			if (_isMaterialFromPool)
			{
				_materialPropertyBlock.SetColor(_hoverColorOverride, unityColor);
				_materialPropertyBlock.SetColor(_proximityLightCenterColorOverride, unityColor);
				_materialPropertyBlock.SetColor(_proximityLightMiddleColorOverride, unityColor);
				_materialPropertyBlock.SetColor(_proximityLightOuterColorOverride, unityColor);

				SetPropertyBlock(setProperty);
			}
			else
			{
				_currentMaterial.SetColor("_HoverColorOverride", unityColor);
				_currentMaterial.SetColor("_ProximityLightCenterColorOverride", unityColor);
				_currentMaterial.SetColor("_ProximityLightMiddleColorOverride", unityColor);
				_currentMaterial.SetColor("_ProximityLightOuterColorOverride", unityColor);
			}
		}

		private void SetPropertyBlock(bool setProperty)
		{
			if (setProperty && _meshRenderer != null)
			{
				_meshRenderer.SetPropertyBlock(_materialPropertyBlock);
			}
		}

		/*
    private bool ApplyMaterialPropertyBlocks(Material material, bool forceApplyMaterialPropertyBlocks)
    {
        bool isMaterialChange = false;

        if (this.meshRenderer.sharedMaterial != material)
        {
            this.currentMaterial = material;
            this.meshRenderer.sharedMaterial = material;

            this.ApplyPropertiesMaterial();

            isMaterialChange = true;
        }
        else
        {
            // On peut avoir envie d'appliquer le MaterialPropertyBlock même si le material n'est pas changé
            if(forceApplyMaterialPropertyBlocks == true)
            {
                this.ApplyPropertiesMaterial();
            }
        }

        return isMaterialChange;
    }
    */

		private void InitializePropertiesMaterial()
		{
			//this.SetTileMode(this.tileMode, false);
			//this.SetLightEnabled(this.isLightEnabled);

			if (_isMaterialFromPool)
			{
				_meshRenderer?.GetPropertyBlock(_materialPropertyBlock);
			}

			SetTileScale(_tileScale, false);
			SetTexture(_mediaTexture, false);
			SetTileOffset(_tileOffset, false);

			ApplyOpacityToColor(false);

			SetLightColor(_lightColor, false);
			SetCullMode(_cullMode, false);

			if (_isMaterialFromPool)
			{
				SetPropertyBlock(true);
			}
		}

		private void SetColor(global::Astrolabe.Twinkle.Color color, bool setProperty)
		{
			_colorWithoutOpacity = color.ToUnityColor();
			ApplyOpacityToColor(setProperty);
		}

		public float GetMixedOpacity()
		{
			var opacityClamped = Mathf.Clamp(_opacity, 0, 1);

			var brushOpacityClamped = 1f;

			if (_currentBrush != null)
			{
				brushOpacityClamped = Mathf.Clamp(_currentBrush.Opacity, 0, 1);
			}

			// L'opacity externe et l'opacity du brush sont prises en compte
			return opacityClamped * brushOpacityClamped;
		}

		private bool IsColorAndOpacityOpaque()
		{
			var finalOpacity = GetMixedOpacity();
			var newColor = _colorWithoutOpacity.GetColorWithOpacity(finalOpacity);

			return newColor.a == 1f;
		}

		private void ApplyOpacityToColor(bool setProperty)
		{
			var finalOpacity = GetMixedOpacity();
			var newColor = _colorWithoutOpacity.GetColorWithOpacity(finalOpacity);

			_materialColor = newColor;

			if (_isMaterialFromPool)
			{
				_materialPropertyBlock.SetColor(_color, newColor);

				SetPropertyBlock(setProperty);
			}
			else
			{
				_currentMaterial.color = newColor;
			}

			// devrait marcher avec les couleurs dont l'alpha est à zero mais ne fonctionne pas apparement
			RefreshRenderer();
		}

		private UnityEngine.Color _materialColor;

		public ShaderMode ShaderMode
		{
			get => _shaderMode;

			set
			{
				if (_shaderMode != value)
				{
					SetShaderMode(value);
				}
			}
		}

		private ShaderMode _shaderMode = ShaderMode.Auto;

		public float Opacity
		{
			get => _userOpacity;

			set
			{
				if (_userOpacity != value)
				{
					_userOpacity = value;
					// this.opacity est clampé de 0 à 1
					// on conserve donc la valeur fournie par le user dans userOpacity
					SetOpacity(value, true);
				}
			}
		}

		private float _userOpacity = 1f;
		private float _opacity = 1f;
		private bool _haveBackground = false;

		private Collider _collider;

		private void RefreshRenderer()
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = CanEnableRenderer;
			}
		}

		private void RefreshCollider()
		{
			if (_collider != null)
			{
				_collider.enabled = CanEnableCollider;
			}
		}

		public bool CanEnableRenderer
		{
			get
			{
				var logicalElement = _rootComponent.VisualElement?.LogicalElement;
				return _haveBackground && _materialColor.a != 0 && _width != 0 && _height != 0 && _depth != 0;
			}
		}

		public bool CanEnableCollider
		{
			get
			{
				if (IsHitTestVisible == true)
				{
					return _haveBackground && _width != 0 && _height != 0 && _depth != 0;
				}

				return false;
			}
		}

		public bool IsHitTestVisible
		{
			get => _isHitTestVisible;

			set
			{
				_isHitTestVisible = value;
				RefreshCollider();
			}
		}

		private bool _isHitTestVisible = true;

		public void SetShaderMode(ShaderMode shaderMode)
		{
			_shaderMode = shaderMode;

			ChangeMaterial(true);
		}

		/// <summary>
		/// Change l'alpha de la couleur (et rapplique la couleur colorWithoutOpacity)
		/// </summary>
		/// <param name="value"></param>
		public void SetOpacity(float value, bool setProperty)
		{
			_opacity = value;

			// opacité externe + celle du brush
			var isOpaque = IsColorAndOpacityOpaque();

			var isMaterialChange = false;

			if (isOpaque == false)
			{
				// ici on traite la transparence
				if (_isTransparencyEnabled == false)
				{
					_isTransparencyEnabled = true;
					ChangeMaterial(true);

					isMaterialChange = true;
				}
			}
			else
			{
				// ici on traite l'opaque
				if (_isTransparencyEnabled == true && BrushType != BrushTypes.Texture)
				{
					_isTransparencyEnabled = false;
					ChangeMaterial(true);

					isMaterialChange = true;
				}
			}

			// pas de changement de materiel on applique le couleur
			if (isMaterialChange == false)
			{
				ApplyOpacityToColor(setProperty);
			}
		}

		/// <summary>
		/// Tile Scale
		/// </summary>
		public global::Astrolabe.Twinkle.Vector2 TileScale
		{
			get => _tileScale;

			set
			{
				if (_tileScale != value)
				{
					SetTileScale(value, true);
				}
			}
		}

		private global::Astrolabe.Twinkle.Vector2 _tileScale = global::Astrolabe.Twinkle.Vector2.One;

		private void SetTileScale(global::Astrolabe.Twinkle.Vector2 value, bool setProperty)
		{
			_tileScale = value;

			float x, y;

			if (value.X == 0)
			{
				x = MAX_TILING_SCALE;
			}
			else
			{
				// là ou le scale du tml = 0.5, le scale de Unity = 2
				x = 1 / value.X;
			}

			if (value.Y == 0)
			{
				y = MAX_TILING_SCALE;
			}
			else
			{
				y = 1 / value.Y;
			}

			var v = new UnityEngine.Vector2(x, y);

			_materialScaleX = x;
			_materialScaleY = y;

			if (float.IsNaN(_materialOffsetX) == false)
			{
				if (_isMaterialFromPool)
				{
					_materialPropertyBlock.SetVector(_mainTexSt,
						new Vector4(_materialScaleX, _materialScaleY, _materialOffsetX, _materialOffsetY));

					SetPropertyBlock(setProperty);
				}
				else
				{
					_currentMaterial.mainTextureScale = v;
				}
			}

			// tileOffset se sert de tileScale pour calculer son offset
			SetTileOffset(_tileOffset, setProperty);
		}

		/// <summary>
		/// Tile Scale
		/// </summary>
		public global::Astrolabe.Twinkle.Vector2 TileOffset
		{
			get => _tileOffset;

			set
			{
				if (_tileOffset != value)
				{
					SetTileOffset(value, true);
				}
			}
		}

		private global::Astrolabe.Twinkle.Vector2 _tileOffset = global::Astrolabe.Twinkle.Vector2.Zero;

		private void SetTileOffset(global::Astrolabe.Twinkle.Vector2 normalizedValue, bool setProperty)
		{
			// sx est ScaleX du Tml; tx est TilingX du shader MRTK
			var sx = _tileScale.X;
			var sy = _tileScale.Y;

			float tx;
			float ty;

			if (sx == 0)
			{
				tx = MAX_TILING_SCALE;
			}
			else
			{
				tx = 1 / sx; // sx == 2 -> tx = 0.5 (tiling compte le nombre de tuile affichées à l'écran)
			}

			if (sy == 0)
			{
				ty = MAX_TILING_SCALE;
			}
			else
			{
				ty = 1 / sy;
			}

			_tileOffset = normalizedValue;

			// alignement centrée
			float startX;
			float startY;

			switch (_alignmentX)
			{
				case AlignmentX.Center:
					startX = (1 - sx) / 2;
					break;

				case AlignmentX.Right:
					startX = 1 - sx;
					break;

				default:
					startX = 0;
					break;
			}

			switch (_alignmentY)
			{
				case AlignmentY.Center:
					startY = (1 - sy) / 2;
					break;

				case AlignmentY.Bottom:
					startY = 1 - sy;
					break;

				default:
					startY = 0;
					break;
			}

			var x = (normalizedValue.X + startX) * tx;
			var y = (normalizedValue.Y + startY) * ty;

			AlignmentOffsetX = startX;
			AlignmentOffsetY = startY;

			var ox = -x;
			var oy = -(ty - 1) + y;

			_materialOffsetX = ox;
			_materialOffsetY = oy;

			if (float.IsNaN(_materialScaleX) == false)
			{
				if (_isMaterialFromPool)
				{
					_materialPropertyBlock.SetVector(_mainTexSt,
						new Vector4(_materialScaleX, _materialScaleY, _materialOffsetX, _materialOffsetY));

					SetPropertyBlock(setProperty);
				}
				else
				{
					var v = new UnityEngine.Vector2(_materialOffsetX, _materialOffsetY);
					_currentMaterial.mainTextureOffset = v;
				}
			}
		}

		private float _materialOffsetX = float.NaN;
		private float _materialOffsetY = float.NaN;
		private float _materialScaleX = float.NaN;
		private float _materialScaleY = float.NaN;

		public float AlignmentOffsetX { get; private set; }

		public float AlignmentOffsetY { get; private set; }

		public TileMode TileMode
		{
			get => _tileMode;

			set
			{
				if (_tileMode != value)
				{
					SetTileMode(value, true);
				}
			}
		}

		private TileMode _tileMode = TileMode.None;

		/// <summary>
		/// Tricks permettant d'appliquer l'affichage des tuiles (en demandant la gestion ou non des Corners). Si corners pas d'affichage.
		/// Attention peut faire changer le material shared si changeMAterial = true
		/// </summary>
		/// <param name="tileMode"></param>
		private void SetTileMode(TileMode tileMode, bool changeMaterial)
		{
			_tileMode = tileMode;

			switch (tileMode)
			{
				case TileMode.None:

					// Tuile non affichées si Corner affiché
					_isRoundCornerEnabled = true;
					break;

				default:

					// tuile affichées si Corner désactivé
					_isRoundCornerEnabled = false;

					break;
			}

			if (BrushType != BrushTypes.Texture)
			{
				return;
			}

			// On ne fait rien si le ImageBrush n'est pas present
			if (_currentBrush == null)
			{
				return;
			}

			if (changeMaterial == true && _imageSource != null)
			{
				// si ImageSource est null il n'y aura pas d'allocation de Texture
				SetImageSource(_imageSource, false);

				// On change le nouveau material (et si c'est vraiment un nouveau on applique le materialPropertyBlock)
				ChangeMaterial(false);
			}

			if (_currentMaterial != null && _currentMaterial.mainTexture != null)
			{
				if (_isRoundCornerEnabled == false)
				{
					_currentMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
				}
				else
				{
					_currentMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
				}
			}
		}

		public AlignmentX AlignmentX
		{
			get => _alignmentX;

			set
			{
				if (_alignmentX != value)
				{
					_alignmentX = value;
					SetAlignment(value, _alignmentY, true);
				}
			}
		}

		public AlignmentY AlignmentY
		{
			get => _alignmentY;

			set
			{
				if (_alignmentY != value)
				{
					_alignmentY = value;
					SetAlignment(_alignmentX, value, true);
				}
			}
		}

		public void SetAlignment(AlignmentX alignmentX, AlignmentY alignmentY, bool setProperty)
		{
			_alignmentX = alignmentX;
			_alignmentY = alignmentY;

			SetTileOffset(_tileOffset, setProperty);
		}

		private AlignmentX _alignmentX = AlignmentX.Center;
		private AlignmentY _alignmentY = AlignmentY.Center;

		public BrushTypes BrushType { get; private set; }

		public float Width
		{
			get => _width;

			set
			{
				if (_width != value)
				{
					_width = value;
					RefreshGameObject();
				}
			}
		}

		private float _width = 0;

		public float Height
		{
			get => _height;

			set
			{
				if (_height != value)
				{
					_height = value;
					RefreshGameObject();
				}
			}
		}

		private float _height = 0;

		public float Depth
		{
			get => _depth;

			set
			{
				if (_depth != value)
				{
					_depth = value;
					RefreshGameObject();
				}
			}
		}

		private float _depth = 0.005f;

		public void SetCullMode(CullMode cullMode, bool setProperty)
		{
			_cullMode = cullMode;

			if (_isMaterialFromPool)
			{
				_materialPropertyBlock.SetFloat(_cull, (float)cullMode);

				SetPropertyBlock(setProperty);
			}
			else
			{
				_currentMaterial.SetFloat("_Cull", (float)cullMode);
			}
		}

		private CullMode _cullMode = CullMode.Off;

		private Texture2D LoadTexture(ImageSource imageSource)
		{
			if (imageSource == null || (imageSource.Source == null && imageSource.Pixels == null))
			{
				return null;
			}

			Texture2D texture = null;

			if (imageSource.Pixels != null)
			{
				// Chargement à partir d'un fichier de byte

				TextureFormat format;

				switch (imageSource.PixelFormat)
				{
					case PixelFormat.BGRA32:
						format = TextureFormat.BGRA32;
						break;

					case PixelFormat.RGB24:
						format = TextureFormat.RGB24;
						break;

					case PixelFormat.RGBA32:
					default:
						format = TextureFormat.RGBA32;
						break;
				}

				var tex2D = new Texture2D((int)imageSource.PixelWidth, (int)imageSource.PixelHeight, format, false);

				tex2D.LoadRawTextureData(imageSource.Pixels);
				tex2D.Apply();

				texture = tex2D;
			}
			else
			{
				// Chargement à partir d'une URL
				// Transform l'uri
				var uriInfo = UriInformation.ParseToRead(imageSource.Source);

				var fullfilename = uriInfo.GetPath();

				// key = Mario.png#Tile ou Mario.png#None";
				var key = fullfilename + "#" + _tileMode.ToString();

				texture = TwinkleApplication.Instance.ObjectCache.GetCache(key) as Texture2D;

				if (texture == null)
				{
					texture = ImageFileHelper.ReadImageFileToTexture(fullfilename);

					if (texture != null)
					{
						texture.wrapMode = _tileMode == TileMode.None ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
						TwinkleApplication.Instance.ObjectCache.AddCache(key, texture);
					}
				}

				if (texture != null)
				{
					imageSource.UpdateSize(texture.width, texture.height);
				}
			}

			return texture;
		}

		/// <summary>
		/// Removes fluent shader object (texture...) from astrolabe ObjectCache
		/// </summary>
		/// <param name="fileName"></param>
		public bool RemoveObjectFromCache(string fileName)
		{
			var key = fileName + "#" + TileMode;
			return TwinkleApplication.Instance.ObjectCache.RemoveCache(key);
		}

		public bool RemoveThisObjectFromCache()
		{
			if (_imageSource != null && _imageSource.Source != null)
			{
				var uriInfo = UriInformation.ParseToRead(_imageSource.Source);
				var fileName = uriInfo.GetPath();
				var key = fileName + "#" + TileMode;

				return TwinkleApplication.Instance.ObjectCache.RemoveCache(key);
			}

			return false;
		}

		/// <summary>
		/// Properly liberating memory space (textures) when disposing a fluent shader
		/// </summary>
		public void Dispose()
		{
			if (_currentMaterial != null)
			{
				_currentMaterial.mainTexture = null;
				_currentMaterial = null;
			}

			if (_meshRenderer != null)
			{
				foreach (var mat in _meshRenderer.materials)
				{
					mat.mainTexture = null;
				}
			}

			_emptyTexture = null;
			_mediaTexture = null;
		}
	}

	public enum BrushTypes
	{
		Color,
		Texture
	}
}