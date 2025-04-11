using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	//Cette classe a été écrite avec l'aide de ChatGPT
	public class VideoFileHelper : MonoBehaviour
	{
		public static (int width, int height) GetMp4Dimensions(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (var reader = new BinaryReader(fs))
			{
				var moovStart = FindMoovAtom(reader);
				if (moovStart == -1)
				{
					return (0, 0);
				}

				fs.Seek(moovStart, SeekOrigin.Begin);
				return ReadMoovAtom(reader);
			}
		}

		private static long FindMoovAtom(BinaryReader reader)
		{
			long position = 0;
			while (position < reader.BaseStream.Length)
			{
				reader.BaseStream.Seek(position, SeekOrigin.Begin);
				var size = ReadInt32(reader);
				var type = ReadString(reader, 4);

				if (type == "moov")
				{
					return position;
				}

				if (size < 8 || position + size > reader.BaseStream.Length) // Invalid atom size
				{
					break;
				}

				position += size;
			}

			return -1;
		}

		private static (int width, int height) ReadMoovAtom(BinaryReader reader)
		{
			long moovSize = ReadInt32(reader);
			var moovType = ReadString(reader, 4);
			var moovEnd = reader.BaseStream.Position + moovSize - 8; // -8 for size and type bytes

			while (reader.BaseStream.Position < moovEnd)
			{
				var currentPos = reader.BaseStream.Position;
				var size = ReadInt32(reader);
				var type = ReadString(reader, 4);

				if (type == "trak")
				{
					var dimensions = ReadTrakAtom(reader, currentPos + size - 8);
					if (dimensions.width > 0 && dimensions.height > 0)
					{
						return dimensions;
					}
				}
				else if (size > 8)
				{
					reader.BaseStream.Seek(currentPos + size, SeekOrigin.Begin);
				}
				else
				{
					break; // invalid atom size
				}
			}

			return (0, 0);
		}

		private static (int width, int height) ReadTrakAtom(BinaryReader reader, long trakEnd)
		{
			while (reader.BaseStream.Position < trakEnd)
			{
				var currentPos = reader.BaseStream.Position;
				var size = ReadInt32(reader);
				var type = ReadString(reader, 4);

				if (type == "tkhd")
				{
					reader.BaseStream.Seek(76, SeekOrigin.Current); // Skip to width and height
					var width = ReadFixedPoint1616(reader);
					var height = ReadFixedPoint1616(reader);
					return (width, height);
				}
				else if (IsContainerAtom(type) && size > 8)
				{
					var dimensions = ReadTrakAtom(reader, currentPos + size - 8); // Recursively search nested atoms
					if (dimensions.width > 0 && dimensions.height > 0)
					{
						return dimensions;
					}
				}
				else if (size > 8)
				{
					reader.BaseStream.Seek(currentPos + size, SeekOrigin.Begin);
				}
				else
				{
					break; // invalid atom size
				}
			}

			return (0, 0); // Return zero dimensions if no valid tkhd atom found
		}

		private static bool IsContainerAtom(string type)
		{
			return type == "trak" || type == "mdia" || type == "minf" || type == "stbl";
		}

		private static int ReadInt32(BinaryReader reader)
		{
			var bytes = reader.ReadBytes(4);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToInt32(bytes, 0);
		}

		private static string ReadString(BinaryReader reader, int length)
		{
			var bytes = reader.ReadBytes(length);
			return Encoding.ASCII.GetString(bytes);
		}

		private static int ReadFixedPoint1616(BinaryReader reader)
		{
			var bytes = reader.ReadBytes(4);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			var value = BitConverter.ToInt32(bytes, 0);
			return value >> 16;
		}
	}
}