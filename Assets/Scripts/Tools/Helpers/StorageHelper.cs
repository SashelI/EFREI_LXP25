using System;
using System.IO;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_ANDROID
using System.IO;
#endif

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace Assets.Scripts.Tools.Helpers
{
	/// <summary>
	/// Classe statique de gestion du stockage qui renvoie les chemins importants de l'application.
	/// Permet notamment de retourner des dossiers spécifiques qui varient d'une plateforme à l'autre.
	/// </summary>
	public class StorageHelper : MonoBehaviour
	{
		private readonly char[] FileForbiddenChars = new char[] { '/', '\\', '<', '>', ':', '"', '|', '?', '*' };

#if !UNITY_EDITOR && UNITY_ANDROID
		private const string DocumentsFolderName = "Documents";
		private const string PicturesFolderName = "Pictures";

		private const string AndroidPersistantDataPathEnd = "/Android/data";
#endif

#if WINDOWS_UWP
		private const string HoloLensSavedPicturesPathEnd = "\\Saved Pictures";
#endif

		public string EmbeddedAssetsPath { get; private set; }
		public string EmbeddedPrimitivesPath { get; private set; }

		public const string LOCAL_ASSETS_CACHE_FOLDER_NAME = "LocalAssetsTemp";
		public const string GALLERY_THUMBNAILS_CACHE_FOLDER_NAME = "ThumbnailsTemp";
		public const string CURRENT_SESSION_FOLDER_NAME = "currentSession";
		public const string EMBEDDED_ASSETS_FOLDER_NAME = "Models";
		public const string EMBEDDED_PRIMITIVES_FOLDER_NAME = "Primitives";

		public static StorageHelper Instance { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier du cache temporaire de l'application
		///		ex. Unity Editor : C:/Users/Synergiz/AppData/Local/Temp/Synergiz/Synergiz Harbor Player
		///		ex. HoloLens : U:/Users/hololens2-05@synergiz.com/AppData/Local/Packages/SynergizHarbor.Player_4p4dn32ayqy9j/TempState
		///		ex. Meta Quest : /storage/emulated/0/Android/data/com.Synergiz.HarborPlayer/cache
		/// </summary>
		public string TemporaryCachePath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier des données persistantes de l'application
		///		ex. Unity Editor : C:/Users/Synergiz/AppData/LocalLow/Synergiz/Synergiz Harbor Player
		///		ex. HoloLens : U:/Users/hololens2-05@synergiz.com/AppData/Local/Packages/SynergizHarbor.Player_4p4dn32ayqy9j/LocalState
		///		ex. Meta Quest : /storage/emulated/0/Android/data/com.Synergiz.HarborPlayer/files
		/// </summary>
		public string PersistentDataPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier des streaming assets de l'application
		///		ex. Unity Editor : C:/Users/Synergiz/Projets/Harbor/BoHK.HololensViewer/Assets/StreamingAssets
		///		ex. HoloLens : C:/data/Programs/WindowsApps/SynergizHarbor.Player_61.0.0.0_arm64__4p4dn32ayqy9j/Data/StreamingAssets
		///		ex. Meta Quest : jar:file:///data/app/~~ky6uE0tTZCAyxkXVVPsDKw==/com.Synergiz.HarborPlayer-aT_YmaqCxu1rOmP3I7un8Q==/base.apk!/assets
		/// </summary>
		public string StreamingAssetsPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier documents de l'utilisateur
		/// Dans le cas de UWP, Objects3D est utilisé car le dossier Documents ne passe pas la validation du store
		///		ex. Unity Editor : C:/Users/Synergiz/Documents
		///		ex. HoloLens : U:/USERS/hololens2-05@synergiz.com/3D Objects
		///		ex. Meta Quest : /storage/emulated/0/Documents
		/// </summary>
		public string DocumentsPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier pictures de l'utilisateur
		///		ex. Unity Editor : C:/Users/Synergiz/Pictures
		///		ex. HoloLens : U:/USERS/hololens2-05@synergiz.com/Pictures
		///		ex. Meta Quest : /storage/emulated/0/Pictures
		/// </summary>
		public string PicturesPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier temporaire des assets locaux de l'application.
		/// Ce dossier sert de transition temporaire pour l'ajout d'assets locaux à une session
		/// </summary>
		public string LocalAssetsTempPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier temporaire des miniatures de la galerie,
		/// pour celles qui ne sont pas fournies à part (les images sont elles-même les miniatures, etc)
		/// </summary>
		public string GalleyThumbnailsTempPath { get; private set; }

		/// <summary>
		/// Renvoie le chemin vers le dossier temporaire currentsession
		/// </summary>
		public string CurrentSessionTempPath { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
			}
			else
			{
				Instance = this;

				TemporaryCachePath = Application.temporaryCachePath;
				PersistentDataPath = Application.persistentDataPath;
				StreamingAssetsPath = Application.streamingAssetsPath;

				DocumentsPath = GetDocumentsDirectoryPath();
				PicturesPath = GetPicturesDirectoryPath();

				LocalAssetsTempPath = Path.Combine(TemporaryCachePath, LOCAL_ASSETS_CACHE_FOLDER_NAME);
				GalleyThumbnailsTempPath = Path.Combine(TemporaryCachePath, GALLERY_THUMBNAILS_CACHE_FOLDER_NAME);
				CurrentSessionTempPath = Path.Combine(TemporaryCachePath, CURRENT_SESSION_FOLDER_NAME);

				EmbeddedAssetsPath = Path.Combine(StreamingAssetsPath, EMBEDDED_ASSETS_FOLDER_NAME);
				EmbeddedPrimitivesPath = Path.Combine(StreamingAssetsPath, EMBEDDED_PRIMITIVES_FOLDER_NAME);

				// On vide le dossier temporaire des assets locaux des sessions précédentes
				if (Directory.Exists(LocalAssetsTempPath))
				{
					Directory.Delete(LocalAssetsTempPath, true);
				}
				Directory.CreateDirectory(LocalAssetsTempPath);

				// Idem pour les miniatures
				if (Directory.Exists(GalleyThumbnailsTempPath))
				{
					Directory.Delete(GalleyThumbnailsTempPath, true);
				}
				Directory.CreateDirectory(GalleyThumbnailsTempPath);
			}
		}

		/// <summary>
		/// Checks if the file already exists and adds a number between parentheses if it does
		/// </summary>
		/// <param name="filePath">Complete path to the file</param>
		public string GetUniqueNameForFileWindowsStyle(string filePath)
		{
			int fileIndex = 1;
			string fileExtension = Path.GetExtension(filePath);

			// Permet de retirer l'extension du chemin
			string baseFilePath = Path.ChangeExtension(filePath, null);

			// Tant que le nom choisi existe déjà dans les captures on augmente son index final
			while (File.Exists(filePath))
			{
				filePath = $"{baseFilePath} ({++fileIndex}){fileExtension}";
			}

			return filePath;
		}

		/// <summary>
		/// Removes forbidden chars from a file name and replaces them if needed
		/// </summary>
		/// <param name="fileName">The file name (not a path)</param>
		/// <param name="replacementChar">The optional string to replace each forbidden char</param>
		/// <returns></returns>
		public string RemoveForbiddenCharsFromFileName(string fileName, string replacementChar = null)
		{
			string[] temp = fileName.Split(FileForbiddenChars, StringSplitOptions.RemoveEmptyEntries);

			return string.Join(replacementChar ?? string.Empty, temp);
		}

		private string GetDocumentsDirectoryPath()
		{
			string documentsPath = null;

#if UNITY_EDITOR
			documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif UNITY_ANDROID
			string rootPathForCustomUserAssets = GetRootPathFromCustomUserAssets();

			if (!string.IsNullOrWhiteSpace(rootPathForCustomUserAssets)
				&& !rootPathForCustomUserAssets.Equals(PersistentDataPath, System.StringComparison.OrdinalIgnoreCase))
			{
				documentsPath = Path.Combine(rootPathForCustomUserAssets, DocumentsFolderName);
			}
#elif WINDOWS_UWP
			documentsPath = KnownFolders.Objects3D.Path;
#endif

			return documentsPath;
		}

		private string GetPicturesDirectoryPath()
		{
			string picturesPath = null;

#if UNITY_EDITOR
			picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
#elif UNITY_ANDROID
			string rootPathForCustomUserAssets = GetRootPathFromCustomUserAssets();

			if (!string.IsNullOrWhiteSpace(rootPathForCustomUserAssets)
				&& !rootPathForCustomUserAssets.Equals(PersistentDataPath, System.StringComparison.OrdinalIgnoreCase))
			{
				picturesPath = Path.Combine(rootPathForCustomUserAssets, PicturesFolderName);
			}
#elif WINDOWS_UWP
			picturesPath = GetPicturesPathFromSavedPicturesAssets();
#endif

			return picturesPath;
		}

#if !UNITY_EDITOR && UNITY_ANDROID
		/// <summary>
		/// Retourne le dossier de l'utilisateur sur Android
		/// </summary>
		private string GetRootPathFromCustomUserAssets()
		{
			// PersistentDataPath: /storage/emulated/<userid>/Android/data/<packagename>/files
			// RootPathFromCustomUserAssets doit être: /storage/emulated/<userid>
			return PersistentDataPath.TrimEndStartsWith(AndroidPersistantDataPathEnd);
		}
#endif

#if WINDOWS_UWP
		/// <summary>
		/// Retourne le dossier des photos sur HoloLens (UWP)
		/// </summary>
		private string GetPicturesPathFromSavedPicturesAssets()
		{
			// SavedPicturesPath: U:/USERS/<userid>/Pictures/Saved Pictures
			// PicturesPathFromSavedPicturesPath doit être: U:/USERS/<userid>/Pictures
			return KnownFolders.SavedPictures.Path.TrimEndStartsWith(HoloLensSavedPicturesPathEnd);
		}
#endif
		private void OnApplicationQuit()
		{
			//On nettoie les dossiers temporaires

			if (Directory.Exists(LocalAssetsTempPath))
			{
				Directory.Delete(LocalAssetsTempPath, true);
			}


			if (Directory.Exists(GalleyThumbnailsTempPath))
			{
				Directory.Delete(GalleyThumbnailsTempPath, true);
			}
		}
	}
}