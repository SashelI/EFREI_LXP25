using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Tools
{
	/// <summary>
	/// Provides a list of static methods to convert objects to and from bytes array
	/// </summary>
	public static class BytesConverters
	{

		/// <summary>
		/// Obtenir une liste de vector3 depuis un byte array
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static List<Vector3> GetListVector3WithByteArray(byte[] bytes)
		{
			string serializedArray = null;

			try
			{
				if (bytes == null || bytes.Length == 0)
				{
					throw new ArgumentNullException("bytes");
				}

				if (!TryParseSerializedVector3Array(serializedArray = Encoding.UTF8.GetString(bytes), out var vectors))
				{
					throw new Exception("Can't parse");
				}

				return vectors;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error while transforming \"" +
					$"{(!string.IsNullOrWhiteSpace(serializedArray) ? serializedArray : "empty array")}\": {ex.Message}");
			}
		}

		/// <summary>
		/// Obtenir le byte array pour le string représentant un array de vector3 sérialisé
		/// </summary>
		/// <param name="serializedArray"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] GetBytesForVector3ArraySerialized(string serializedArray)
		{
			try
			{
				if (!TryParseSerializedVector3Array(serializedArray, out var _))
				{
					throw new Exception("Can't parse");
				}

				return Encoding.UTF8.GetBytes(serializedArray);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error while transforming \"{serializedArray}\": {ex.Message}");
			}
		}

		/// <summary>
		/// Obtenir un byte array depuis une liste de vector3. On va sérialiser l'info et renvoyer un string
		/// </summary>
		/// <param name="vectors"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static byte[] GetBytesForListVector3(List<Vector3> vectors)
		{
			try
			{
				if (vectors != null && vectors.Any())
				{
					string serializedArray = GetSerializedStringForListVector3(vectors);

					return Encoding.UTF8.GetBytes(serializedArray);
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error while transforming vector3 list: {ex.Message}");
			}
		}

		/// <summary>
		/// Retourner une valeur sérialisée (string) pour une liste de Vector3
		/// </summary>
		/// <param name="vectors"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string GetSerializedStringForListVector3(List<Vector3> vectors)
		{
			try
			{
				if (vectors != null && vectors.Any())
				{
					string serializedArray = string.Join(";",
						vectors
						.Select(
							v => $"<" +
							$"{v.x.ToString("G", CultureInfo.InvariantCulture)}," +
							$"{v.y.ToString("G", CultureInfo.InvariantCulture)}," +
							$"{v.z.ToString("G", CultureInfo.InvariantCulture)}>"));

					return serializedArray;
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error while transforming vector3 list: {ex.Message}");
			}
		}

		/// <summary>
		/// TryParse d'un array de vector3 sérialisé ("<x,y,z>;<x,y,z>")
		/// </summary>
		/// <param name="serializedVector3Array"></param>
		/// <param name="vectors"></param>
		/// <returns></returns>
		private static bool TryParseSerializedVector3Array(string serializedVector3Array, out List<Vector3> vectors)
		{
			bool badSerialization = false;

			if (string.IsNullOrWhiteSpace(serializedVector3Array))
			{
				badSerialization = true;
			}

			string[] serializedVectors = serializedVector3Array.Split(';');

			if (serializedVectors == null || serializedVectors.Length == 0)
			{
				badSerialization = true;
			}

			vectors = new List<Vector3>();

			foreach (var serializedVector in serializedVectors)
			{
				var serializedVectorArrayCleaned =
					serializedVector
						.TrimStart('<').TrimEnd('>')
						.Split(',');

				if (serializedVectorArrayCleaned == null || serializedVectorArrayCleaned.Length == 0)
				{
					badSerialization = true;
					break;
				}

				if (!float.TryParse(serializedVectorArrayCleaned[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
				{
					badSerialization = true;
				}

				if (!float.TryParse(serializedVectorArrayCleaned[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
				{
					badSerialization = true;
				}

				if (!float.TryParse(serializedVectorArrayCleaned[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
				{
					badSerialization = true;
				}

				vectors.Add(new Vector3(x, y, z));
			}

			if (badSerialization)
			{
				vectors = null;
				return false;
			}

			return true;
		}


		/// <summary>
		/// Get bytes from a <see cref="Vector3"/>
		/// </summary>
		/// <param name="vec3">The <see cref="Vector3"/> to convert in <see cref="byte[]"/></param>
		/// <returns>A <see cref="byte[]"/> representing <paramref name="vec3"/>, with the bytes in x, y and z order</returns>
		public static byte[] GetBytes(Vector3 vec3)
		{
			byte[] xBytes = GetBytes(vec3.x);
			byte[] yBytes = GetBytes(vec3.y);
			byte[] zBytes = GetBytes(vec3.z);

			byte[] fusion = xBytes.Concat(yBytes).Concat(zBytes).ToArray();

			return fusion;
		}

		public static Vector3 ToVector3(byte[] bytes)
		{
			const int sizeOfFloat = sizeof(float);

			if (bytes.Length != sizeOfFloat * 3)
			{
				throw new InvalidCastException("The size of the byte array is incorrect");
			}

			float x = BitConverter.ToSingle(bytes, 0);
			float y = BitConverter.ToSingle(bytes, sizeOfFloat);
			float z = BitConverter.ToSingle(bytes, sizeOfFloat * 2);

			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Get bytes from a <see cref="Quaternion"/>
		/// </summary>
		/// <param name="quat">The <see cref="Quaternion"/> to convert in <see cref="byte[]"/></param>
		/// <returns>A <see cref="byte[]"/> representing <paramref name="quat"/>, with the bytes in x, y, z and w order</returns>
		public static byte[] GetBytes(Quaternion quat)
		{
			byte[] xBytes = GetBytes(quat.x);
			byte[] yBytes = GetBytes(quat.y);
			byte[] zBytes = GetBytes(quat.z);
			byte[] wBytes = GetBytes(quat.w);

			byte[] fusion = xBytes.Concat(yBytes).Concat(zBytes).Concat(wBytes).ToArray();

			return fusion;
		}

		public static Quaternion ToQuaternion(byte[] bytes)
		{
			const int sizeOfFloat = sizeof(float);

			if (bytes.Length != sizeOfFloat * 4)
			{
				throw new InvalidCastException("The size of the byte array is incorrect");
			}

			float x = BitConverter.ToSingle(bytes, 0);
			float y = BitConverter.ToSingle(bytes, sizeOfFloat);
			float z = BitConverter.ToSingle(bytes, sizeOfFloat * 2);
			float w = BitConverter.ToSingle(bytes, sizeOfFloat * 3);

			return new Quaternion(x, y, z, w);
		}

		/// <summary>
		/// Get bytes from a <see cref="float"/>
		/// </summary>
		/// <param name="f">The <see cref="float"/> to convert in <see cref="byte[]"/></param>
		/// <returns>A <see cref="byte[]"/> representing <paramref name="f"/></returns>
		public static byte[] GetBytes(float f)
		{
			return BitConverter.GetBytes(f);
		}
	}
}