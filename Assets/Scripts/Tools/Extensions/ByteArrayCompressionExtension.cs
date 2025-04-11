using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Assets.Scripts.Tools.Extensions
{
	public static class ByteArrayCompressionExtension
	{    /// <summary>
		 /// Compress a byte array
		 /// </summary>
		 /// <param name="data">The byte array to compress</param>
		 /// <returns>The compressed byte array</returns>
		public async static Task<byte[]> CompressAsync(this byte[] data)
		{
			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
			{
				await dstream.WriteAsync(data, 0, data.Length);
			}

			return output.ToArray();
		}

		/// <summary>
		/// Decompress a byte array
		/// </summary>
		/// <param name="data">The byte array to decompress</param>
		/// <returns>The decompressed byte array</returns>
		public async static Task<byte[]> DecompressAsync(this byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			MemoryStream output = new MemoryStream();
			using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
			{
				await dstream.CopyToAsync(output);
			}

			return output.ToArray();
		}
	}
}