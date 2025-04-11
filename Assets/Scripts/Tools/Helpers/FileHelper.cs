using System;
using System.IO;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Linq;
using System.Text.RegularExpressions;
#endif

namespace Assets.Scripts.Tools.Helpers
{
	/// <summary>
	/// Gestion fichiers
	/// </summary>
	public static class FileHelper
	{
		/// <summary>
		/// Nom du fichier contenant les chemins vers les 
		/// </summary>
		public const string STREAMINGASSETS_FILENAME = "streamingassetspath";

		public static string[] streamingAssetsDirectories = { "Models", "Primitives" };

		/// <summary>
		/// Chemin complet vers le fichier des chemins des différents fichiers StreamingAsset
		/// </summary>
		public static string StreamingAssetsResourceFilePath
		{
			get
			{
				return Path.Combine(Application.dataPath, "Resources", STREAMINGASSETS_FILENAME + ".txt");
			}
		}

#if UNITY_ANDROID && !UNITY_EDITOR

		/// <summary>
		/// Obtenir la liste des chemins de fichiers correspondant aux fichiers dans StreamingAssets
		/// </summary>
		/// <returns></returns>
		public static string[] GetPathsFromStreaminAssetsListing()
		{
			TextAsset txtAsset = Resources.Load<TextAsset>(STREAMINGASSETS_FILENAME);

			if (txtAsset != null)
			{
				var lines = Regex.Split(txtAsset.text, "\n|\r|\r\n");

				if (!lines.IsNullOrCountZero())
				{
					for(int i=0; i<lines.Length; i++) 
					{
						lines[i] = Path.Combine(Application.streamingAssetsPath, lines[i]); //Pour avoir un URI correcte pour la webrequest qui va suivre
					}
					return lines;
				}
			}

			return null;
		}
#endif
		/// <summary>
		/// Obtenir tous les fichiers d'un répertoire
		/// Gestion avec Android
		/// </summary>
		/// <param name="directoryPath"></param>
		/// <param name="streamingAssetsRootPath">Détermine si on veut récupérer les fichiers listés dans StreamingAssets. Si oui, on gère pour Android</param>
		/// <param name="filterExtension"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string[] GetAllFiles(string directoryPath, bool streamingAssetsRootPath = false, string filterExtension = "")
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentNullException(nameof(directoryPath));
			}

#if UNITY_ANDROID && !UNITY_EDITOR
			// Pour l'instant uniquement pour les fichiers se trouvant dans StreamingAssets
			if (streamingAssetsRootPath)
			{
				// On va utiliser le potentiel fichier de listing des StreamingAssets
				var filePaths = GetPathsFromStreaminAssetsListing();

				if (!filePaths.IsNullOrCountZero())
				{
					var filteredFilePaths = filePaths;

					if (filteredFilePaths.Any(f => f.StartsWith(directoryPath)))
					{
						filteredFilePaths = filteredFilePaths.Where(f => f.StartsWith(directoryPath)).ToArray();
					}

					if (!string.IsNullOrWhiteSpace(filterExtension))
					{
						filteredFilePaths =
							filteredFilePaths?.Where(f => Path.GetExtension(f).Equals(filterExtension, StringComparison.OrdinalIgnoreCase))?.ToArray();
					}
					return filteredFilePaths;
				}
			}

			return null;

#else
			if (Directory.Exists(directoryPath))
			{
				return Directory.GetFiles(directoryPath);
			}
			else
			{
				return Array.Empty<string>();
			}
#endif

		}
	}
}
