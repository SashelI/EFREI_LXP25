using System;
using System.IO;
using System.Threading.Tasks;
using Assets.Astrolabe.Libs.OBJImport;
using GLTFast;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	/// <summary>
	/// Gestion des fichiers images
	/// </summary>
	internal static class Model3DFileHelper
	{
		public static async Task<GameObject> ReadModel3DFileAsync(string fullfilename,
			global::Astrolabe.Twinkle.ShaderMode shaderMode, bool addCollider = true)
		{
			if (fullfilename == null)
			{
				return null;
			}

			var extension = Path.GetExtension(fullfilename)?.ToLower();

			GameObject root = null;

			switch (extension)
			{
				case ".obj":
					var pathObj = fullfilename;
					var pathMtl = Path.ChangeExtension(pathObj, ".mtl");

					root = new OBJLoader().Load(pathObj, pathMtl, shaderMode);
					break;

				case ".gltf":
				case ".glb":
					//var gltfObject = new GLTFLoader(shaderMode, fullfilename);
					//root = await gltfObject.Load();

					// Utilisation de GltFast désormais, pour coller à Harbor notamment
					root = new GameObject(Path.GetFileName(fullfilename));
					var gltf = new GltfImport();

					var success = await gltf.LoadFile(fullfilename);

					if (success)
					{
						await gltf.InstantiateMainSceneAsync(root.transform);
					}

					break;

				default:
					throw new Exception("This model3D file format '" + fullfilename + "' is not supported yet!");
			}

			if (root != null && addCollider == true)
			{
				// Rajoute les meshs colliders
				var count = root.transform.childCount;

				for (var i = 0; i < count; i++)
				{
					var child = root.transform.GetChild(i);

					child.gameObject.AddComponent<MeshCollider>();
				}
			}

			return root;
		}
	}
}