using System;
using System.Collections.Generic;

namespace Assets.Scripts.Tools.Extensions
{
	public static class ByteArraySliceExtension
	{    /// <summary>
		 /// Slice an array into chunks of the specified size
		 /// </summary>
		 /// <param name="data">The byte array to compress</param>
		 /// <param name="idealChunkSize">The size of each chunk. The last chunk can have a smaller size</param>
		 /// <returns>The compressed byte array</returns>
		public static IList<byte[]> Slice(this byte[] data, int idealChunkSize)
		{
			IList<byte[]> chunks = new List<byte[]>();

			byte[] buffer;
			for (int i = 0; i < data.Length; i += idealChunkSize)
			{
				int remainingSize = data.Length - i;
				Console.WriteLine(remainingSize);

				int chunkSize = idealChunkSize;

				if (remainingSize <= idealChunkSize)
				{
					chunkSize = remainingSize;
				}

				buffer = new byte[chunkSize];
				Array.Copy(data, i, buffer, 0, chunkSize);

				chunks.Add(buffer);
			}

			return chunks;
		}
	}
}