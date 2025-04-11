using Astrolabe.Twinkle;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Astrolabe.Scripts
{
	public class TwinkleMaterialFactory : MonoBehaviour
	{
		public static TwinkleMaterialFactory Instance { get; set; }

		// T = Transparent RC = Round Corner L = Light
		// T = bit 2 / RC = bit 1 / L = bit 0 
		[FormerlySerializedAs("MaterialT0RC0L0")] public Material materialT0Rc0L0;
		[FormerlySerializedAs("MaterialT0RC0L1")] public Material materialT0Rc0L1;
		[FormerlySerializedAs("MaterialT0RC1L0")] public Material materialT0Rc1L0;
		[FormerlySerializedAs("MaterialT0RC1L1")] public Material materialT0Rc1L1;
		[FormerlySerializedAs("MaterialT1RC0L0")] public Material materialT1Rc0L0;
		[FormerlySerializedAs("MaterialT1RC0L1")] public Material materialT1Rc0L1;
		[FormerlySerializedAs("MaterialT1RC1L0")] public Material materialT1Rc1L0;
		[FormerlySerializedAs("MaterialT1RC1L1")] public Material materialT1Rc1L1;

		// Instance New à cloner
		[FormerlySerializedAs("MaterialT0NewInstance")] public Material materialT0NewInstance;
		[FormerlySerializedAs("MaterialT1NewInstance")] public Material materialT1NewInstance;

		// Material Border Rectangle
		[FormerlySerializedAs("MaterialT0Border")] public Material materialT0Border;
		[FormerlySerializedAs("MaterialT1Border")] public Material materialT1Border;

		// Material pour les objets 3D (Clonage)
		[FormerlySerializedAs("MaterialT13D")] public Material materialT13D;
		[FormerlySerializedAs("MaterialT03D")] public Material materialT03D;

		private Material[] _materialPool;

		[FormerlySerializedAs("EmptyTexture")] public Texture2D emptyTexture;

		public Material GetMaterialFromPool(bool isTransparencyEnabled, bool isRoundCornerEnabled, bool isLightEnabled,
			ShaderMode shaderMode)
		{
			// Force à prendre en charge le modèle opaque ou Opacity s'il n'est pas en mode Auto
			if (shaderMode != ShaderMode.Auto)
			{
				isTransparencyEnabled = shaderMode != ShaderMode.Opaque;
			}

			var index = 0;

			if (isTransparencyEnabled)
			{
				index += 4;
			}

			if (isRoundCornerEnabled)
			{
				index += 2;
			}

			if (isLightEnabled)
			{
				index++;
			}

			return _materialPool[index];
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				_materialPool = new Material[8];
			}

			emptyTexture = new Texture2D(0, 0);

			Debug.Assert(_materialPool != null, "materialPool is null!");
			;

			_materialPool[0] = materialT0Rc0L0;
			_materialPool[1] = materialT0Rc0L1;
			_materialPool[2] = materialT0Rc1L0;
			_materialPool[3] = materialT0Rc1L1;
			_materialPool[4] = materialT1Rc0L0;
			_materialPool[5] = materialT1Rc0L1;
			_materialPool[6] = materialT1Rc1L0;
			_materialPool[7] = materialT1Rc1L1;

			// Lancement de AstrolabeManager synchronisé avec AstrolabeManager
			AstrolabeManager.InitializeApplication();
		}

		public Material CreateOrGetNewInstanceMaterial(Brush currentBrush, bool isTransparency, ShaderMode shaderMode)
		{
			if (currentBrush == null)
			{
				return null;
			}

			if (currentBrush.InstanceMethod != InstanceMethods.New)
			{
				return null;
			}

			// Force à prendre en charge le modèle opaque ou Opacity s'il n'est pas en mode Auto
			if (shaderMode != ShaderMode.Auto)
			{
				isTransparency = shaderMode != ShaderMode.Opaque;
			}

			var brushInstanceMaterial = currentBrush.Instance as BrushInstanceMaterial;

			if (brushInstanceMaterial == null)
			{
				brushInstanceMaterial = new BrushInstanceMaterial();
				currentBrush.Instance = brushInstanceMaterial;
			}

			Material material;

			if (isTransparency)
			{
				if (brushInstanceMaterial.MaterialTransparency == null)
				{
					brushInstanceMaterial.MaterialTransparency = Instantiate(materialT1NewInstance);
				}

				material = brushInstanceMaterial.MaterialTransparency;
			}
			else
			{
				if (brushInstanceMaterial.MaterialOpaque == null)
				{
					brushInstanceMaterial.MaterialOpaque = Instantiate(materialT0NewInstance);
				}

				material = brushInstanceMaterial.MaterialOpaque;
			}

			return material;
		}

		public static Material Clone(ShaderMode shaderMode)
		{
			switch (shaderMode)
			{
				case ShaderMode.Opaque:
					return Instantiate(Instance.materialT03D);

				// automatique et opacity
				default:
					return Instantiate(Instance.materialT13D);
			}
		}

		public void DisposeMaterial(Brush brush)
		{
			if (brush != null && brush.Instance != null)
			{
				var brushInstanceMaterial = brush.Instance as BrushInstanceMaterial;

				if (brushInstanceMaterial.MaterialOpaque != null)
				{
					Destroy(brushInstanceMaterial.MaterialOpaque);
				}

				if (brushInstanceMaterial.MaterialTransparency != null)
				{
					Destroy(brushInstanceMaterial.MaterialTransparency);
				}

				brush.Instance = null;
			}
		}

		public class BrushInstanceMaterial
		{
			public Material MaterialTransparency { get; set; }
			public Material MaterialOpaque { get; set; }
		}
	}
}