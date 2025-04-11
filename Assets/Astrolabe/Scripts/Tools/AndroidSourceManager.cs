using System.IO;
using Astrolabe.Twinkle;
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Networking;
using Astrolabe.Diagnostics;
#endif

namespace Assets.Astrolabe.Scripts.Tools
{
	/// <summary>
	/// Path management for SVG Astrolabe images.
	/// On Android, we cannot open a file stream (and thus open a svg with skia) inside streaming assets folder.
	/// So Foundation needs to move those files on build to keep Core workflow intact.
	/// </summary>
	public class AndroidSourceManager : IAndroidSourceManager
	{
		public AndroidSourceManager()
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			IsSourceCopiedToPersistent = true; //We don't need this boolean if not on android build
#endif
		}

		public bool IsSourceCopiedToPersistent { get; private set; } = false;

		public string MediaSource
		{
			get => _mediaSource;
			set
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				if (!string.IsNullOrWhiteSpace(value))
				{
					_oldImageSource = value.Replace('\\', Path.DirectorySeparatorChar);
					_oldImageSource = _oldImageSource.Replace('/', Path.DirectorySeparatorChar);

					var filename = Path.GetFileName(value);
					_mediaSource = Path.Combine(ANDROID_TARGET_DIRECTORY, filename);

					_mediaSource = _mediaSource.Replace('\\', Path.DirectorySeparatorChar);
					_mediaSource = _mediaSource.Replace('/', Path.DirectorySeparatorChar);

					try
					{
						MoveSvgFromStreamingAssets();
					}
					catch (Exception e)
					{
						IsSourceCopiedToPersistent = true;
						Log.WriteLine($"{e.Message} \n {e.StackTrace}", LogMessageType.Error);
					}
				} 
#else
				if (!string.IsNullOrWhiteSpace(value))
				{
					_mediaSource = value.Replace('\\', Path.DirectorySeparatorChar);
					_mediaSource = _mediaSource.Replace('/', Path.DirectorySeparatorChar);
				}
#endif
			}
		}
		private string _mediaSource=string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
		private string _oldImageSource = string.Empty;
		private readonly string ANDROID_TARGET_DIRECTORY = Path.Combine(Application.persistentDataPath, "AstrolabeSvgSources");

		private void MoveSvgFromStreamingAssets()
		{
			if (!string.IsNullOrWhiteSpace(_oldImageSource) && !string.IsNullOrWhiteSpace(_mediaSource))
			{
				var destFolder = Path.GetDirectoryName(_mediaSource);

				if (!string.IsNullOrWhiteSpace(destFolder) && !Directory.Exists(destFolder))
				{
					Directory.CreateDirectory(destFolder);
				}

				if (!File.Exists(_mediaSource))
				{
					using (var request = UnityWebRequest.Get(_oldImageSource))
					{
						request.SendWebRequest();

						while (!request.isDone)
						{
							if (request.result == UnityWebRequest.Result.ConnectionError)
							{
								throw new Exception($"Connection Error while reading File '{_oldImageSource}'");
							}
						}
						if (request.result == UnityWebRequest.Result.ConnectionError)
						{
							throw new Exception($"Connection Error while reading File '{_oldImageSource}'");
						}
						if (request.result == UnityWebRequest.Result.Success)
						{
							File.WriteAllBytes(_mediaSource, request.downloadHandler.data);
							Log.WriteLine($".svg file source copied from {_oldImageSource} to {_mediaSource}.");
						}
						else
						{
							Log.WriteLine($"Error while copying .svg file source from {_oldImageSource} to {_mediaSource} !", LogMessageType.Error);
						}
					}
				}
			}
			IsSourceCopiedToPersistent = true;
		}
#endif
		public PixelFormat GetPixelFormat()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			return PixelFormat.RGBA32;
#else
			return PixelFormat.BGRA32;
#endif
		}
	}
}

