#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Tools.Extensions
{
	/// <summary>
	/// Extensions pour le répertoire des StreamingAssets
	/// </summary>
	public static class StreamingAssetsExtension
	{
		/// <summary>
		/// Obtenir la totalité des chemins des fichiers contenus dans StreamingAssets
		/// Ne fonctionne qu'en mode éditeur et va permettre de récupérer des fichiers directement avec leur chemin (pour Android)
		/// </summary>
		/// <param name="path">Relatif à Application.streamingAssetsPath.</param>
		/// <param name="paths">Liste des chemins</param>
		/// <returns></returns>
		public static List<string> GetPathsRecursively(string path, ref List<string> paths)
		{
			var fullPath = Path.Combine(Application.streamingAssetsPath, path);
			DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
			foreach (var file in dirInfo.GetFiles())
			{
				if (!file.Extension.Contains(".meta"))
				{
					var rightPath = Path.Combine(path, file.Name);
#if UNITY_ANDROID
					rightPath = Regex.Replace(rightPath, @"\\", "/");
#endif
					paths.Add(Path.Combine(rightPath, file.Name)); // With file extension
				}
			}

			foreach (var dir in dirInfo.GetDirectories())
			{
				GetPathsRecursively(Path.Combine(path, dir.Name), ref paths);
			}
			return paths;
		}
		public static List<string> GetPathsRecursively(List<string> paths, ref List<string> filePaths)
		{
			foreach (string path in paths)
			{
				var fullPath = Path.Combine(Application.streamingAssetsPath, path);
				DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
				foreach (var file in dirInfo.GetFiles())
				{
					if (!file.Extension.Contains(".meta"))
					{
						var filePath = Path.Combine(path, file.Name);
#if UNITY_ANDROID
						filePath = Regex.Replace(filePath, @"\\", "/");
#endif
						filePaths.Add(filePath); // With file extension
					}
				}
				if (!string.IsNullOrEmpty(path))
				{
					foreach (var dir in dirInfo.GetDirectories())
					{
						GetPathsRecursively(Path.Combine(path, dir.Name), ref filePaths);
					}
				}
			}

			return filePaths;
		}

		public static List<string> GetPathsRecursively(string path)
		{
			List<string> paths = new List<string>();
			return GetPathsRecursively(path, ref paths);
		}

		public static List<string> GetPathsRecursively(List<string> paths)
		{
			List<string> filePaths = new List<string>();
			return GetPathsRecursively(paths, ref filePaths);
		}
	}
}
#endif
