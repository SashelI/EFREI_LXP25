using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Astrolabe.Diagnostics;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using UnityEngine.Networking;
#elif ENABLE_WINMD_SUPPORT
using Astrolabe.Core.Uwp.IO;
using Astrolabe.Twinkle;
#endif

namespace Assets.Astrolabe.Scripts.Tools
{
	/// <summary>
	/// Emplacement de stockage
	/// </summary>
	public enum StorageType
	{
		Application,
		Persistent,
		StreamingAssets,
		Temporary,

		// ne rien faire
		AbsoluteFile
	}

	/// <summary>
	/// Gestion de fichiers
	/// </summary>
	public static class FileHelper
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		private const string ANDROID_JAR_PREFIX = "jar:";
		private const string ANDROID_FILE_PREFIX = "file://";
#endif

		public static async Task<bool> CheckBroadFileAccessAsync(string filename)
		{
#if ENABLE_WINMD_SUPPORT
            return await BroadFileSystemAccess.CheckFileAccessAsync(filename);
#else
			await Task.CompletedTask;
			return true;
#endif
		}

		public static async Task<bool> CheckBroadFolderAccessAsync(string foldername)
		{
#if ENABLE_WINMD_SUPPORT
            return await BroadFileSystemAccess.CheckFolderAccessAsync(foldername);
#else
			await Task.CompletedTask;
			return true;
#endif
		}

		public static async Task LaunchBroadAccessFileSystemSettingsAsync()
		{
#if ENABLE_WINMD_SUPPORT
            await BroadFileSystemAccess.LaunchBroadAccessFileSystemSettingsAsync();
#endif
		}

		/// <summary>
		/// Crée un dossier si il n'existe pas
		/// </summary>
		/// <param name="fullPath">String - chemin complet du dossier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static bool CreateFolderIfMissing(string fullPath)
		{
			try
			{
#if ENABLE_WINMD_SUPPORT
                if(TwinkleApplication.Instance.IsDesignMode)
                {
                    return AbsolutePathFileHelper.CreateFolderIfMissing(fullPath);
                }
                else
                {
#endif
				if (!FolderExists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}
#if ENABLE_WINMD_SUPPORT
                }
#endif
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);

				return false;
			}

			return true;
		}

		/// <summary>
		/// Crée un dossier si il n'existe pas
		/// </summary>
		/// <param name="storage">StorageType - Emplacement du dossier</param>
		/// <param name="path">String - dossier a tester</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static bool CreateFolderIfMissing(StorageType storage, string path)
		{
			var absolutePath = Path.Combine(storage.GetPath(), path);

			return CreateFolderIfMissing(absolutePath);
		}

		/// <summary>
		/// Enregistre du texte dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteTextAsync(string content, string fullPath,
			bool overwriteIfExist = true)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                try
                {
                    _ = await AbsolutePathFileHelper.CreateFolderIfMissingAsync(Path.GetDirectoryName(fullPath));

                    if (overwriteIfExist)
                    {
                        await AbsolutePathFileHelper.WriteAllTextAsync(fullPath, content);
                    }
                    else
                    {
                        await AbsolutePathFileHelper.AppendAllTextAsync(fullPath, content);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex);
                    return ex;
                }

                return null;
            }
            else
            {
#endif
			return await Task.Run(() =>
			{
				try
				{
					_ = CreateFolderIfMissing(Path.GetDirectoryName(fullPath));

					if (overwriteIfExist)
					{
						File.WriteAllText(fullPath, content);
					}
					else
					{
						File.AppendAllText(fullPath, content);
					}
				}
				catch (Exception ex)
				{
					Log.WriteLine(ex);
					return ex;
				}

				return null;
			});
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		public static Exception WriteText(string content, string fullPath, bool overwriteIfExist = true)
		{
			try
			{
				_ = CreateFolderIfMissing(Path.GetDirectoryName(fullPath));

#if ENABLE_WINMD_SUPPORT
                if(TwinkleApplication.Instance.IsDesignMode)
                {
                    if (overwriteIfExist)
                    {
                        AbsolutePathFileHelper.WriteAllText(fullPath, content);
                    }
                    else
                    {
                        AbsolutePathFileHelper.AppendAllText(fullPath, content);
                    }
                }
                else
                {
#endif

				if (overwriteIfExist)
				{
					File.WriteAllText(fullPath, content);
				}
				else
				{
					File.AppendAllText(fullPath, content);
				}
#if ENABLE_WINMD_SUPPORT
                }
#endif
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return ex;
			}

			return null;
		}

		/// <summary>
		/// Enregistre du content binaire dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteBytesAsync(byte[] content, string fullPath)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                try
                {
                    await AbsolutePathFileHelper.CreateFolderIfMissingAsync(Path.GetDirectoryName(fullPath));
                    await AbsolutePathFileHelper.WriteAllBytesAsync(fullPath, content);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex);
                    return ex;
                }

                return null;
            }
            else
            {
#endif
			return await Task.Run<Exception>(() => { return WriteBytes(content, fullPath); });
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		public static Exception WriteBytes(byte[] content, string fullPath)
		{
			try
			{
				CreateFolderIfMissing(Path.GetDirectoryName(fullPath));
				File.WriteAllBytes(fullPath, content);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return ex;
			}

			return null;
		}

		/// <summary>
		/// Enregistre du texte dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteTextAsync(string content, string fileName, string path,
			bool overwriteIfExist = true)
		{
			return await WriteTextAsync(content, Path.Combine(path, fileName), overwriteIfExist);
		}

		/// <summary>
		/// Enregistre du texte dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteTextAsync(string content, string fileName, StorageType storageType,
			string subFolder = "", bool overwriteIfExist = true)
		{
			try
			{
				var path = Path.Combine(storageType.GetPath(), subFolder, fileName);
				return await WriteTextAsync(content, path, overwriteIfExist);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return ex;
			}
		}

		/// <summary>
		/// Enregistre du texte dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static Exception WriteText(string content, string fileName, string path, bool overwriteIfExist = true)
		{
			return WriteText(content, Path.Combine(path, fileName), overwriteIfExist);
		}

		/// <summary>
		/// Enregistre du texte dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static Exception WriteText(string content, string fileName, StorageType storageType,
			string subFolder = "", bool overwriteIfExist = true)
		{
			try
			{
				var path = Path.Combine(storageType.GetPath(), subFolder, fileName);
				return WriteText(content, path, overwriteIfExist);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return ex;
			}
		}

		/// <summary>
		/// Enregistre du contenu binaire dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteBytesAsync(byte[] content, string fileName, string path)
		{
			return await WriteBytesAsync(content, Path.Combine(path, fileName));
		}

		/// <summary>
		/// Enregistre du contenu binaire dans un fichier
		/// </summary>
		/// <param name="content">string - contenu texte du fichier</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="overwriteIfExist">booléen - si TRUE écrase le fichier existant, si FALSE ajoute au fichier existant</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteBytesAsync(byte[] content, string fileName, StorageType storageType,
			string subFolder = "")
		{
			return await WriteBytesAsync(content, Path.Combine(storageType.GetPath(), subFolder, fileName));
		}

		public static Exception WriteBytes(byte[] content, string fileName, StorageType storageType,
			string subFolder = "")
		{
			var path = storageType.GetPath();
			return WriteBytes(content, Path.Combine(path, subFolder, fileName));
		}

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static ReadFileResult<string> ReadAllText(string fullPath)
		{
			try
			{
#if ENABLE_WINMD_SUPPORT
                if (TwinkleApplication.Instance.IsDesignMode)
                {
                    return new ReadFileResult<string>(AbsolutePathFileHelper.ReadAllText(fullPath));
                }
                else
                {
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
				return new ReadFileResult<string>(GetAndroidFileText(fullPath));
#else

				return new ReadFileResult<string>(File.ReadAllText(fullPath));
#endif


#if ENABLE_WINMD_SUPPORT
                }
#endif
			}
			catch (Exception ex)
			{
				Log.WriteLine("Exception Type" + ex.GetType() + " Message=" + ex.Message);

				return new ReadFileResult<string>(ex);
			}
		}

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static ReadFileResult<string> ReadAllText(string fileName, string path)
		{
			return ReadAllText(Path.Combine(path, fileName));
		}

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static ReadFileResult<string> ReadAllText(string fileName, StorageType storageType,
			string subFolder = "")
		{
			return ReadAllText(Path.Combine(storageType.GetPath(), subFolder, fileName));
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		public static void GetMemoryStreamFromPathAndroid(string path, bool fromStreamingAssets, out MemoryStream memoryStream)
		{
			if (!fromStreamingAssets)
			{
				path = path.Insert(0, ANDROID_FILE_PREFIX);
			}

			byte[] fileData;
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(path))
			{
				ProcessAndroidFileRetrieval(unityWebRequest, path);

				fileData = unityWebRequest.downloadHandler.data;
			}

			memoryStream = new MemoryStream(fileData);
		}

		/// <summary>
		/// Obtenir une valeur string d'un fichier sur plateforme Android (utilisation de UnityWebRequest)
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		private static string GetAndroidFileText(string fullPath)
		{
			string text = null;
			if (!fullPath.StartsWith(ANDROID_JAR_PREFIX))
			{
				fullPath = fullPath.Insert(0, ANDROID_FILE_PREFIX);
			}
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(fullPath))
			{
				ProcessAndroidFileRetrieval(unityWebRequest, fullPath);

				text = unityWebRequest.downloadHandler.text;
			}

			return text;
		}

		/// <summary>
		/// Obtenir une valeur byte[] d'un fichier sur plateforme Android (utilisation de UnityWebRequest) 
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		private static byte[] GetAndroidFileBytes(string fullPath)
		{
			byte[] bytes = null;
			if (!fullPath.StartsWith(ANDROID_JAR_PREFIX))
			{
				fullPath = fullPath.Insert(0, ANDROID_FILE_PREFIX);
			}
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(fullPath))
			{
				ProcessAndroidFileRetrieval(unityWebRequest, fullPath);

				bytes = unityWebRequest.downloadHandler.data;
			}

			return bytes;
		}

		/// <summary>
		/// Processer la requête de récupération d'un fichier sur Android
		/// </summary>
		/// <param name="unityWebRequest"></param>
		/// <param name="fullPath"></param>
		/// <exception cref="Exception"></exception>
		private static void ProcessAndroidFileRetrieval(UnityWebRequest unityWebRequest, string fullPath)
		{
			unityWebRequest.SendWebRequest();

			while (!unityWebRequest.isDone)
			{
				if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
				{
					throw new Exception($"Connection Error while reading File '{fullPath}'");
				}
			}

			if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
			{
				throw new Exception($"Connection Error while reading File '{fullPath}'");
			}
		}
#endif

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static async Task<ReadFileResult<string>> ReadAllTextAsync(string fullPath)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                try
                {
                    return new ReadFileResult<string>(await AbsolutePathFileHelper.ReadAllTextAsync(fullPath));
                }
                catch (Exception ex)
                {
                    return new ReadFileResult<string>(ex);
                }
            }
            else
            {
#endif
			return await Task.Run(() =>
			{
				try
				{
#if UNITY_ANDROID && !UNITY_EDITOR
					return new ReadFileResult<string>(GetAndroidFileText(fullPath));
#else

					return new ReadFileResult<string>(File.ReadAllText(fullPath));
#endif
				}
				catch (Exception ex)
				{
					return new ReadFileResult<string>(ex);
				}
			});
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static async Task<ReadFileResult<string>> ReadAllTextAsync(string fileName, string path)
		{
			return await ReadAllTextAsync(Path.Combine(path, fileName));
		}

		/// <summary>
		/// Lit le contenu d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>ReadFileResult.Content - String - Contenu du fichier dans un string</returns>
		public static async Task<ReadFileResult<string>> ReadAllTextAsync(string fileName, StorageType storageType,
			string subFolder = "")
		{
			var path = Path.Combine(storageType.GetPath(), subFolder, fileName);
			return await ReadAllTextAsync(path);
		}

		/// <summary>
		/// Lit toutes les lignes d'un fichier texte
		/// </summary>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <returns>ReadFileResult.Content - String[] - Toutes les lignes du fichier dans un tableau de string</returns>
		public static async Task<ReadFileResult<string[]>> ReadAllLinesAsync(string fullPath)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                try
                {
                    return new ReadFileResult<String[]>(await AbsolutePathFileHelper.ReadAllLinesAsync(fullPath));
                }
                catch (Exception ex)
                {
                    return new ReadFileResult<String[]>(ex);
                }
            }
            else
            {
#endif
			return await Task.Run(() =>
			{
				try
				{
#if UNITY_ANDROID && !UNITY_EDITOR
					string text = GetAndroidFileText(fullPath);

					if (string.IsNullOrWhiteSpace(text))
					{
						throw new Exception("Empty file");
					}

					return new ReadFileResult<string[]>(text.Split('\n'));
#else

					return new ReadFileResult<string[]>(File.ReadAllLines(fullPath));
#endif
				}
				catch (Exception ex)
				{
					return new ReadFileResult<string[]>(ex);
				}
			});
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		/// <summary>
		/// Lit toutes les lignes d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <returns>ReadFileResult.Content - String[] - Toutes les lignes du fichier dans un tableau de string</returns>
		public static async Task<ReadFileResult<string[]>> ReadAllLinesAsync(string fileName, string path)
		{
			return await ReadAllLinesAsync(Path.Combine(path, fileName));
		}

		/// <summary>
		/// Lit toutes les lignes d'un fichier texte
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>ReadFileResult.Content - String[] - Toutes les lignes du fichier dans un tableau de string</returns>
		public static async Task<ReadFileResult<string[]>> ReadAllLinesAsync(string fileName, StorageType storageType,
			string subFolder = "")
		{
			return await ReadAllLinesAsync(Path.Combine(storageType.GetPath(), subFolder, fileName));
		}

		/// <summary>
		/// Lit le contenu d'un fichier binaire
		/// </summary>
		/// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
		/// <returns>ReadFileResult.Content - byte[] - Contenu du fichier dans un tableau de bytes</returns>
		public static async Task<ReadFileResult<byte[]>> ReadAllBytesAsync(string fullPath)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                try
                {
                    return new ReadFileResult<byte[]>(await AbsolutePathFileHelper.ReadAllBytesAsync(fullPath));
                }
                catch (Exception ex)
                {
                    return new ReadFileResult<byte[]>(ex);
                }
            }
            else
            {
#endif
			return await Task.Run(() =>
			{
				try
				{
#if UNITY_ANDROID && !UNITY_EDITOR
					return new ReadFileResult<byte[]>(GetAndroidFileBytes(fullPath));
#else

					return new ReadFileResult<byte[]>(File.ReadAllBytes(fullPath));
#endif
				}
				catch (Exception ex)
				{
					return new ReadFileResult<byte[]>(ex);
				}
			});
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		public static ReadFileResult<byte[]> ReadAllBytes(string fullPath)
		{
			try
			{
#if ENABLE_WINMD_SUPPORT
                if(TwinkleApplication.Instance.IsDesignMode)
                {
                    return new ReadFileResult<byte[]>(AbsolutePathFileHelper.ReadAllBytes(fullPath));
                }
                else
                {
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
				return new ReadFileResult<byte[]>(GetAndroidFileBytes(fullPath));
#else

				return new ReadFileResult<byte[]>(File.ReadAllBytes(fullPath));
#endif
#if ENABLE_WINMD_SUPPORT
                }
#endif
			}
			catch (Exception ex)
			{
				return new ReadFileResult<byte[]>(ex);
			}
		}

		/// <summary>
		/// Lit le contenu d'un fichier binaire
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">String - chemin du dossier d'enregistrement</param>
		/// <returns>ReadFileResult.Content - byte[] - Contenu du fichier dans un tableau de bytes</returns>
		public static ReadFileResult<byte[]> ReadAllBytes(string fileName, string path)
		{
			return ReadAllBytes(Path.Combine(path, fileName));
		}

		/// <summary>
		/// Lit le contenu d'un fichier binaire
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storageType">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>ReadFileResult.Content - byte[] - Contenu du fichier dans un tableau de bytes</returns>
		public static ReadFileResult<byte[]> ReadAllBytes(string fileName, StorageType storageType,
			string subFolder = "")
		{
			return ReadAllBytes(Path.Combine(storageType.GetPath(), subFolder, fileName));
		}

		public static bool FolderExists(string folderPath)
		{
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                return AbsolutePathFileHelper.FolderExists(folderPath);
            }
            else
            {
#endif
			return Directory.Exists(folderPath);
#if ENABLE_WINMD_SUPPORT
            }
#endif
		}

		/// <summary>
		/// Vérifie si un dossier existe, si non renvoi le chemin vers le dossier de données
		/// </summary>
		/// <param name="folder">string - dossier a tester</param>
		/// <param name="storage">StorageType - dossier de données a renvoyer si le dossier n'existe pas</param>
		/// <returns></returns>
		public static string GetFolderOrRoot(string folder, StorageType storage = StorageType.Persistent)
		{
			if (string.IsNullOrWhiteSpace(folder) || FolderExists(folder) == false)
			{
				folder = storage.GetPath();
			}

			return folder;
		}

		/// <summary>
		/// Vide un dossier de données, ne fonctionne pas avec les dossier StreamingAssets et Application
		/// </summary>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<bool> ClearStorageAsync(StorageType storage, string subFolder = "")
		{
			try
			{
				if (storage == StorageType.Persistent || storage == StorageType.Temporary)
				{
					await ClearFolderAsync(Path.Combine(storage.GetPath(), subFolder));
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Vide un dossier et tous ses fichiers récursivement
		/// </summary>
		/// <param name="path">String - dossier a vider</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<bool> ClearFolderAsync(string path)
		{
			await Task.Run(() =>
			{
				try
				{
					if (FolderExists(path))
					{
						Directory.Delete(path, true);
					}
				}
				catch (Exception ex)
				{
					Log.WriteLine(ex);
					return false;
				}

				return true;
			});
			return true;
		}

		#region GetInfos

		/// <summary>
		/// Renvoi le FileInfo correspondant au fichier si celui ci existe
		/// </summary>
		/// <param name="fullPath">String - Chemin complet du fichier (dossier + fichier)</param>
		/// <returns>FileInfo - information du fichier</returns>
		public static FileInfo GetFileInfo(string fullPath)
		{
			FileInfo returnValue = null;
			if (File.Exists(fullPath))
			{
				returnValue = new FileInfo(fullPath);
			}

			return returnValue;
		}

		/// <summary>
		/// Renvoi le FileInfo correspondant au fichier si celui ci existe
		/// </summary>
		/// <param name="fileName">string - nom du fichier</param>
		/// <param name="path">string - dossier du fichier</param>
		/// <returns>FileInfo - information du fichier</returns>
		public static FileInfo GetFileInfo(string fileName, string path)
		{
			return GetFileInfo(Path.Combine(GetFolderOrRoot(path), fileName));
		}

		/// <summary>
		/// Renvoi le FileInfo correspondant au fichier si celui ci existe
		/// </summary>
		/// <param name="fileName">string - nom du fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>FileInfo - information du fichier</returns>
		public static FileInfo GetFileInfo(string fileName, StorageType storage, string subFolder = "")
		{
			return GetFileInfo(Path.Combine(storage.GetPath(), subFolder, fileName));
		}

		/// <summary>
		/// Renvoi le DirectoryInfo correspondant au dossier si il existe
		/// </summary>
		/// <param name="fullPath">String - Chemin complet dossier</param>
		/// <returns>DirectoryInfo - information du dossier</returns>
		public static DirectoryInfo GetDirectoryInfo(string fullPath)
		{
			DirectoryInfo returnValue = null;
			if (FolderExists(fullPath))
			{
				returnValue = new DirectoryInfo(fullPath);
			}

			return returnValue;
		}

		/// <summary>
		/// Renvoi le DirectoryInfo correspondant au dossier si il existe
		/// </summary>
		/// <param name="storage">StorageType - Emplacement du dossier</param>
		/// <param name="subFolder">String - dossier ou sous-dossier a tester</param>
		/// <returns>DirectoryInfo - information du dossier</returns>
		public static DirectoryInfo GetDirectoryInfo(StorageType storage, string subFolder = "")
		{
			return GetDirectoryInfo(Path.Combine(storage.GetPath(), subFolder));
		}

		#endregion GetInfos

		#region FindIn

		#region Méthodes Génériques

		/// <summary>
		/// Cherche dans un dossier et ses sous-dossiers tous les fichiers dont le nom contient le texte donné et renvoi la liste de leurs chemins complets
		/// </summary>
		/// <param name="directory">String - Dossier cible</param>
		/// <param name="fileName">String - texte a chercher</param>
		/// <param name="includeDirectory">booléen - inclus les dossiers dans la recherche</param>
		/// <returns>SearchFilesResult.Result - List<String> - Liste des chemins complet de tous les fichier trouvés</returns>
		public static async Task<SearchFilesResult> SearchFiles(string fileName, string directory,
			bool includeDirectory = false)
		{
			return await Task.Run(() =>
			{
				var result = new SearchFilesResult();
				try
				{
					result.Result.AddRange(Directory.EnumerateFiles(directory, fileName, SearchOption.AllDirectories));
					if (includeDirectory)
					{
						result.Result.AddRange(Directory.EnumerateDirectories(directory, fileName,
							SearchOption.AllDirectories));
					}
				}
				catch (Exception e)
				{
					result.ErrorMessage = e.Message;
					return result;
				}

				return result;
			});
		}

		/// <summary>
		/// Résultat d'une recherche de fichiers
		/// </summary>
		public class SearchFilesResult
		{
			/// <summary>
			/// TRUE si la recherche a rencontré un problème
			/// </summary>
			public bool IsError => string.IsNullOrWhiteSpace(ErrorMessage);

			/// <summary>
			/// Message d'erreur si il y a eu un problème
			/// </summary>
			public string ErrorMessage { get; set; } = string.Empty;

			/// <summary>
			/// nombre de résultat trouvé
			/// </summary>
			public int Count => Result.Count;

			/// <summary>
			/// Résultat de la requête
			/// </summary>
			public List<string> Result { get; set; } = new();
		}

		/// <summary>
		/// Teste si un fichier est présent dans le dossier cible ou ses sous-dossiers
		/// </summary>
		/// <param name="directory">String - Dossier cible</param>
		/// <param name="assetName">String - texte a chercher</param>
		/// <param name="includeDirectory">bool - inclus les dossiers dans la recherche</param>
		/// <returns>boolean - True si le fichier est présent</returns>
		public static bool Exists(string fullFilename)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			UnityWebRequest.Result result;
			string requestError = string.Empty;

			if (!fullFilename.StartsWith(ANDROID_JAR_PREFIX))
			{
				fullFilename = fullFilename.Insert(0, ANDROID_FILE_PREFIX);
			}
			using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(fullFilename))
			{
				ProcessAndroidFileRetrieval(unityWebRequest, fullFilename);

				result = unityWebRequest.result;
				requestError = unityWebRequest.error;
			}

			if (result == UnityWebRequest.Result.Success)
			{
				return true;
			}
			else
			{
				Log.WriteLine(requestError, LogMessageType.Error);
				return false;
			}
#else
#if ENABLE_WINMD_SUPPORT
            if(TwinkleApplication.Instance.IsDesignMode)
            {
                return AbsolutePathFileHelper.FileExists(fullFilename);
            }
            else
            {
#endif
			return File.Exists(fullFilename);
#if ENABLE_WINMD_SUPPORT
            }
#endif
#endif
		}

		#endregion Méthodes Génériques

		#region StorageType

		/// <summary>
		/// Cherche dans un dossier et ses sous-dossiers tous les fichiers dont le nom contient le texte donné et renvoi la liste de leurs chemins complets
		/// </summary>
		/// <param name="assetName">String - texte a chercher</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="includeDirectory">bool - inclus les dossiers dans la recherche</param>
		/// <returns>FindFilesResult.Result - List<String> - Liste des chemins complet de tous les fichier trouvés</returns>
		public static async Task<SearchFilesResult> FindAllInDirectory(string assetName, StorageType storage,
			string subFolder = "", bool includeDirectory = false)
		{
			return await SearchFiles(assetName, Path.Combine(storage.GetPath(), subFolder), includeDirectory);
		}

		#endregion StorageType

		#endregion FindIn

		public class ReadFileResult<T>
		{
			/// <summary>
			/// Erreur du système
			/// </summary>
			public bool HasError => Error != null;

			/// <summary>
			/// Code réponse Http
			/// </summary>
			public Exception Error { get; set; }

			/// <summary>
			/// Contenu récupéré
			/// </summary>
			public T Content { get; set; }

			public ReadFileResult()
			{
			}

			public ReadFileResult(Exception ex)
			{
				Error = ex;
			}

			public ReadFileResult(T content)
			{
				Content = content;
			}
		}
	}
}