using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations
{
	public class UnityFileOperations : IFileOperations
	{
		public async Task<ResultFileRead<byte[]>> ReadAllBytesAsync(string filename, FileSource source)
		{
			try
			{
				var fullFilename = source.GetPath(filename);

				var result = await FileHelper.ReadAllBytesAsync(fullFilename);

				if (result.HasError)
				{
					return new ResultFileRead<byte[]>(result.Error);
				}

				return new ResultFileRead<byte[]>(result.Content);
			}
			catch (Exception ex)
			{
				return new ResultFileRead<byte[]>(ex);
			}
		}

		public ResultFileRead<byte[]> ReadAllBytes(string filename, FileSource source)
		{
			try
			{
				var fullFilename = source.GetPath(filename);

				var result = FileHelper.ReadAllBytes(fullFilename);

				if (result.HasError)
				{
					return new ResultFileRead<byte[]>(result.Error);
				}

				return new ResultFileRead<byte[]>(result.Content);
			}
			catch (Exception ex)
			{
				return new ResultFileRead<byte[]>(ex);
			}
		}

		public async Task<Exception> WriteAllBytesAsync(byte[] content, string filename, FileSource source)
		{
			return await FileHelper.WriteBytesAsync(content, filename, StorageType.Persistent, "");
		}

		public Exception WriteAllBytes(byte[] content, string filename, FileSource source, bool overwrite = true)
		{
			return WriteAllBytes(content, filename, source, overwrite);
		}

		public async Task<ResultFileRead<string>> ReadAllTextAsync(string filename, FileSource source)
		{
			try
			{
				var fullFilename = source.GetPath(filename);

				var result = await FileHelper.ReadAllTextAsync(fullFilename);

				if (result.HasError)
				{
					return new ResultFileRead<string>(result.Error);
				}

				return new ResultFileRead<string>(result.Content);
			}
			catch (Exception ex)
			{
				return new ResultFileRead<string>(ex);
			}
		}

		public ResultFileRead<string> ReadAllText(string filename, FileSource source)
		{
			try
			{
				var fullFilename = source.GetPath(filename);

				var result = FileHelper.ReadAllText(fullFilename);

				if (result.HasError)
				{
					return new ResultFileRead<string>(result.Error);
				}

				return new ResultFileRead<string>(result.Content);
			}
			catch (Exception ex)
			{
				return new ResultFileRead<string>(ex);
			}
		}

		public async Task<Exception> WriteAllTextAsync(string text, string filename, FileSource source,
			bool overwrite = true)
		{
			return await FileHelper.WriteTextAsync(text, filename, source.GetStorageType(), "", overwrite);
		}

		public Exception WriteAllText(string text, string filename, FileSource source, bool overwrite = true)
		{
			return FileHelper.WriteText(text, filename, source.GetStorageType(), "", overwrite);
		}

		public bool Exists(string filename, FileSource source)
		{
			var fullFilename = source.GetPath(filename);
			return FileHelper.Exists(fullFilename);
		}

		public string GetFullFilename(UriInformation uriInformation)
		{
			return uriInformation.FileSource.GetPath(uriInformation.Path);
		}
	}
}