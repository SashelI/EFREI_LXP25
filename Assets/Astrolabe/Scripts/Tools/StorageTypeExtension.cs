using System.IO;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class StorageTypeExtension
	{
		private static string _dataPath;
		private static string _persistentDataPath;
		private static string _streamingAssetsPath;
		private static string _temporaryCachePath;

		/// <summary>
		/// Initialisation a appeler sur le MainThread
		/// </summary>
		public static void Initialize()
		{
			_dataPath = Application.dataPath;
			_persistentDataPath = Application.persistentDataPath;
			_streamingAssetsPath = Application.streamingAssetsPath;
			_temporaryCachePath = Application.temporaryCachePath;
		}

		/// <summary>
		/// Renvoi le chemin correspondant
		/// </summary>
		/// <param name="storageType"></param>
		/// <returns></returns>
		public static string GetPath(this StorageType storageType)
		{
			var returnValue = string.Empty;

			switch (storageType)
			{
				case StorageType.Application:
					returnValue = _dataPath;
					break;

				case StorageType.AbsoluteFile:
					returnValue = null;
					break;

				case StorageType.Persistent:
					returnValue = _persistentDataPath;
					break;

				case StorageType.StreamingAssets:

					if (TwinkleApplication.Instance.IsDesignMode)
					{
						returnValue = Path.Combine(TwinkleApplication.Instance.DesignPath, "Assets\\StreamingAssets");
					}
					else
					{
						returnValue = _streamingAssetsPath;
					}

					break;

				case StorageType.Temporary:
					returnValue = _temporaryCachePath;
					break;

				default:
					break;
			}

			return returnValue;
		}
	}
}