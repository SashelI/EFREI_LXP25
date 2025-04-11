using System;
using System.IO;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations
{
	public static class FileSourceExtension
	{
		public static StorageType GetStorageType(this FileSource fileSource)
		{
			switch (fileSource)
			{
				case FileSource.Application:
					return StorageType.StreamingAssets;
				case FileSource.AbsoluteFile:
					return StorageType.AbsoluteFile;
				case FileSource.Storage:
					return StorageType.Persistent;
				default:
					throw new Exception("FileSource conversion unknown!");
			}
		}

		public static string GetPath(this FileSource fileSource, string filename)
		{
			if (fileSource == FileSource.None)
			{
				throw new Exception($"The path \"{filename}\" is not supported!");
			}

			string path = null;

			if (fileSource == FileSource.AbsoluteFile)
			{
				path = filename;
			}
			else
			{
				path = fileSource.GetStorageType().GetPath();
			}

			switch (fileSource)
			{
				case FileSource.AbsoluteFile:
					// renvoie file:///...
					return filename;

				case FileSource.Application:
					return Path.Combine(path, "App", filename);

				default:
					return Path.Combine(path, filename);
			}
		}

		public static string GetPath(this UriInformation uriInfo)
		{
			return uriInfo.FileSource.GetPath(uriInfo.Path);
		}

		public static string GetFullFilename(this Uri uri, bool isReadOnly = true)
		{
			UriInformation uriInfo = null;

			if (isReadOnly)
			{
				uriInfo = UriInformation.ParseToRead(uri);
			}
			else
			{
				uriInfo = UriInformation.ParseToWrite(uri);
			}

			return uriInfo.GetPath();
		}
	}
}