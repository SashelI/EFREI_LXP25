using System;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Renderers;
using Astrolabe.Twinkle.Tools;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = Astrolabe.Twinkle.Vector3;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualModel3D : VisualElement, IVisualModel3D
	{
		public static void LinkToVisualFactory()
		{
			VisualElementFactory.Instance.Add(typeof(LogicalModel3D), typeof(VisualModel3D));
		}

		public VisualModel3D(ILogicalElement logicalElement) : base(logicalElement)
		{
			_model3D = Object.Instantiate(TwinklePrefabFactory.Instance.prefabModel3D).GetComponent<LayoutModel3D>();
			TwinkleComponent = _model3D;
		}

		private readonly LayoutModel3D _model3D;

		public float Depth { get; set; }

		public Model3DSource Source => _source;

		private Model3DSource _source;

		public void SetSource(Model3DSource value, Action<Model3DSource> loaded)
		{
			if (_source != value)
			{
				_source = value;

				LoadModel3D(value, (gameObjectLoaded) =>
				{
					// on le gère le parent
					_model3D.SetObject3D(gameObjectLoaded);
					loaded?.Invoke(value);
				});
			}
		}

		private async void LoadModel3D(Model3DSource model, Action<GameObject> completed)
		{
			try
			{
				if (model == null || model.Source == null)
				{
					Log.WriteLine($"Model3DSource is null!");

					completed?.Invoke(null);
					return;
				}

				GameObject gameObjectLoaded;

				if (model.IsLoaded == false)
				{
					// Transform l'uri
					var fullfilename = UriInformation.ParseToRead(model.Source).GetPath();
					gameObjectLoaded = await Model3DFileHelper.ReadModel3DFileAsync(fullfilename, ShaderMode);
				}
				else
				{
					gameObjectLoaded = (GameObject)model.Content;
				}

				if (gameObjectLoaded != null)
				{
					gameObjectLoaded.SetActive(false);

					if (model.IsLoaded == false)
					{
						var renderers = gameObjectLoaded.GetComponentsInChildren<Renderer>();

						if (renderers != null && renderers.Length > 0)
						{
							/*
                            var positionOriginalWord = model3D.Object3D.position;
                            var rotationOriginalWord = model3D.Object3D.eulerAngles;
                            var scaleOriginalWord = model3D.Object3D.localScale;

                            var parent = model3D.Object3D.parent;
                            model3D.Object3D.parent = null;

                            model3D.Object3D.position = UnityEngine.Vector3.zero;
                            model3D.Object3D.rotation = UnityEngine.Quaternion.identity;
                            model3D.Object3D.localScale = UnityEngine.Vector3.one;
                            */

							// calcul le bounds global de tous les objets enfants
							var bounds = renderers[0].bounds;

							foreach (var renderer in renderers)
							{
								bounds.Encapsulate(renderer.bounds);
							}

							/*
                            model3D.Object3D.parent = parent;

                            model3D.Object3D.position = positionOriginalWord;
                            model3D.Object3D.eulerAngles = rotationOriginalWord;
                            model3D.Object3D.localScale = scaleOriginalWord;
                            */

							model.Width = bounds.size.x.ToLogical();
							model.Height = bounds.size.y.ToLogical();
							model.Depth = bounds.size.z.ToLogical();

							model.Center = bounds.center.ToVector3().ToLogical();
							model.Ray = bounds.extents.magnitude.ToLogical();

							model.Content = gameObjectLoaded;

							_model3D.IsModelLoaded = true;
						}
					}
				}

				completed?.Invoke(gameObjectLoaded);

				gameObjectLoaded.SetActive(true);

				return;
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
			}

			_model3D.IsModelLoaded = false;

			completed?.Invoke(null);

			return;
		}

		public ZAlignment ZAlignment { get; set; }

		public ShaderMode ShaderMode { get; set; }

		public override void Resize(Box renderBox)
		{
			if (_source != null && _source.IsLoaded == true)
			{
				//Taille max du crane en x,y,z -> 29,7
				//1/29.7 -> 0.034 taille ramené à 1
				//taille la plus petite dans le Resize (cette methode)
				//ici 0.05 en y (qui represente le scale Y)
				//0.034*0.05 => 0,001708 = taille du scale proportionnelle dans Objet3D

				//Rayon (renderer.bounds.extents.magnitude) -> 20.5
				//0.05*20.5/29.7 = minRenderResize * transform / sourceMaxSize => pour utiliser les transforms du crane

				var sourceWidth = _source.Width.ToVisual();
				var sourceHeight = _source.Height.ToVisual();
				var sourceDepth = _source.Depth.ToVisual();
				var sourceRay = _source.Ray.ToVisual();
				var sourceCenter = _source.Center.ToVisual();

				var renderMinSize = Mathf.Min(renderBox.Width, renderBox.Height);
				float sourceMaxSize = sourceMaxSize = Mathf.Max(sourceWidth, sourceHeight);

				// en mode Ray on va diminuer la taille pour que le model3D rentre dans une sphere de rayon sourceRay (pratique pour que le rotation ne touche pas les bords)
				var ratioWidth = sourceWidth == 0 ? 0 : renderBox.Width / sourceWidth;
				var ratioHeight = sourceHeight == 0 ? 0 : renderBox.Height / sourceHeight;
				var scaleSize = Mathf.Min(ratioWidth, ratioHeight);

				/*
                if(ZAlignment == ZAlignment.Ray)
                {
                    sourceMaxSize = Mathf.Max(sourceMaxSize, sourceDepth);

                    var ratioWidthDepth = sourceWidth == 0 ? 0 : renderRect.Width / sourceDepth;
                    var ratioHeightDepth = sourceHeight == 0 ? 0 : renderRect.Height / sourceDepth;

                    scaleSize = Mathf.Min( Mathf.Min( scaleSize, ratioWidthDepth), ratioHeightDepth);
                }
                */

				_model3D.Width = scaleSize;
				_model3D.Height = scaleSize;
				_model3D.Depth = scaleSize;

				Vector3 center;

				if (sourceMaxSize == 0 || scaleSize == 0)
				{
					center = Vector3.Zero;
				}
				else
				{
					center = sourceCenter * renderMinSize / sourceMaxSize / scaleSize;
				}

				_model3D.SetCenter(center);

				float halfDepth = 0;
				float ray = 0;

				if (sourceMaxSize != 0)
				{
					var scaleZ = Scale.Z;
					halfDepth = sourceDepth * renderMinSize / sourceMaxSize * scaleZ / 2;
					ray = sourceRay * renderMinSize / sourceMaxSize * scaleZ;
				}

				float positionZ = 0;

				switch (ZAlignment)
				{
					case ZAlignment.Ray:
						positionZ += ray;
						break;

					case ZAlignment.HalfDepth:
						positionZ += halfDepth;
						break;
				}

				// centre de la renderBox (mais pas Z)
				var centerRenderBox = new Vector3(
					renderBox.Location.X + renderBox.Width / 2,
					renderBox.Location.Y + renderBox.Height / 2,
					renderBox.Location.Z + positionZ
				);

				Locate(centerRenderBox);
				ResizeOverride(renderBox);
			}
		}
	}
}