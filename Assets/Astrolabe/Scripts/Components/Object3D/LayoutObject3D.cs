using Assets.Astrolabe.Scripts.Tools;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Components.Object3D
{
	public class LayoutObject3D : TwinkleComponent
	{
		private const string OBJECT_3D_RENDERER_NAME = "Object3DRenderer";

		private MaterialPropertyBlock _materialPropertyBlock;
		protected Transform object3DTransform;
		protected Transform object3DRenderer;

		private FluentShader _fluentShader = new();

		public bool IsDepthCentered { get; set; } = false;

		protected override void SetWidthOverride(float width)
		{
			var scale = object3DTransform.localScale;
			object3DTransform.localScale = new UnityEngine.Vector3(width, scale.y, scale.z);

			FluentShader.Width = width;
		}

		protected override void SetHeightOverride(float height)
		{
			var scale = object3DTransform.localScale;
			object3DTransform.localScale = new UnityEngine.Vector3(scale.x, height, scale.z);

			FluentShader.Height = height;
		}

		protected override void SetDepthOverride(float depth)
		{
			var scale = object3DTransform.localScale;
			object3DTransform.localScale = new UnityEngine.Vector3(scale.x, scale.y, depth);

			if (IsDepthCentered == false)
			{
				var position = renderTransformRotationScale.localPosition;
				renderTransformRotationScale.localPosition = new UnityEngine.Vector3(position.x, position.y, -depth / 2);
			}

			FluentShader.Depth = depth;
		}

		protected virtual Transform GetObject3DTransform()
		{
			return null;
		}

		// Start is called before the first frame update
		protected override void AwakeOverride()
		{
			object3DRenderer = transform.FindRecursively(OBJECT_3D_RENDERER_NAME);

			// permet de dissocier l'objet3D rendu de l'objet qui gère sa position
			// C'est interessant pour Cylindre ou Capsule qui doivent être scalées pour remplir le volume 1x1x1
			object3DTransform = GetObject3DTransform();

			if (object3DTransform == null)
			{
				object3DTransform = object3DRenderer;
			}

			Debug.Assert(object3DTransform != null, "No objectRenderer found!");

			var meshRenderer = object3DRenderer.GetComponent<MeshRenderer>();

			Debug.Assert(meshRenderer != null, "No meshRenderer found!");

			var collider = object3DRenderer.GetComponent<Collider>();

			EnsureRenderTransformExists();

			SetDepth(Depth);

			_materialPropertyBlock = new MaterialPropertyBlock();

			_fluentShader.Initialize(
				this,
				_materialPropertyBlock,
				meshRenderer,
				collider);
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