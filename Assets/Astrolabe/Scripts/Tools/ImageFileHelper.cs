using System;
using System.IO;
using System.Threading.Tasks;
using Astrolabe.Diagnostics;
using UnityEngine;
using static Assets.Astrolabe.Scripts.Tools.FileHelper;

namespace Assets.Astrolabe.Scripts.Tools
{
	/// <summary>
	/// Gestion des fichiers images
	/// </summary>
	public class ImageFileHelper
	{
		#region Fichier vers Image Unity

		/// <summary>
		/// Crée un Sprite à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="pixelsPerUnit">float - Resolution en pixel par unités</param>
		/// <returns>ReadFileResult.Content - Sprite - le Sprite crée, null si le fichier n'existe pas</returns>
		public static async Task<Sprite> ReadImageFileToSpriteAsync(string fullPath, float pixelsPerUnit = 100.0f)
		{
			try
			{
				if (Exists(fullPath))
				{
					var result = await ReadImageFileToTextureAsync(fullPath);
					if (result == null)
					{
						Log.WriteLine(new Exception("Error during file reading"));
						return null;
					}

					return Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2(0, 0),
						pixelsPerUnit);
				}
				else
				{
					Log.WriteLine(new FileNotFoundException());
					return null;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return null;
			}
		}

		/// <summary>
		/// Crée un Sprite à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">string - dossier d'enregistrement du fichier</param>
		/// <param name="pixelsPerUnit">float - Resolution en pixel par unités</param>
		/// <returns>ReadFileResult.Content - Sprite - le Sprite crée, null si le fichier n'existe pas</returns>
		public static async Task<Sprite> ReadImageFileToSpriteAsync(string fileName, string path,
			float pixelsPerUnit = 100.0f)
		{
			return await ReadImageFileToSpriteAsync(Path.Combine(GetFolderOrRoot(path), fileName), pixelsPerUnit);
		}

		/// <summary>
		/// Crée un Sprite à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="pixelsPerUnit">float - Resolution en pixel par unités</param>
		/// <returns>ReadFileResult.Content - Sprite - le Sprite crée, null si le fichier n'existe pas</returns>
		public static async Task<Sprite> ReadImageFileToSpriteAsync(string fileName, StorageType storage,
			string subFolder = "", float pixelsPerUnit = 100.0f)
		{
			return await ReadImageFileToSpriteAsync(Path.Combine(storage.GetPath(), subFolder, fileName),
				pixelsPerUnit);
		}

		/// <summary>
		/// Crée une texture à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <returns>ReadFileResult.Content - Texture2D - La Texture2D crée, null si le fichier n'existe pas</returns>
		public static async Task<Texture2D> ReadImageFileToTextureAsync(string fullPath)
		{
			return await Task.Run(() => { return ReadImageFileToTexture(fullPath); });
		}

		public static Texture2D ReadImageFileToTexture(string fullPath)
		{
			// Load a PNG or JPG file from disk to a Texture2D
			// Returns null if load fails
			Texture2D tex2D = null;

			try
			{
				if (Exists(fullPath))
				{
					var result = ReadAllBytes(fullPath);

					tex2D = new Texture2D(2, 2); // Create new "empty" texture

					if (!tex2D.LoadImage(result.Content))
					{
						// Load the image data into the texture (size is set automatically)
						Log.WriteLine("Image data is not readable", LogMessageType.Error);
					}
				}
				else
				{
					Log.WriteLine("File " + fullPath + " not found", LogMessageType.Error);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				return tex2D;
			}

			return tex2D; // If data = readable -> return texture
		}

		/// <summary>
		/// Crée une texture à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">string - dossier d'enregistrement du fichier</param>
		/// <returns>ReadFileResult.Content - Texture2D - La Texture2D crée, null si le fichier n'existe pas</returns>
		public static async Task<Texture2D> ReadImageFileToTextureAsync(string fileName, string path)
		{
			return await ReadImageFileToTextureAsync(Path.Combine(GetFolderOrRoot(path), fileName));
		}

		/// <summary>
		/// Crée une texture à partir d'un fichier image (PNG ou JPG)
		/// </summary>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <returns>ReadFileResult.Content - Texture2D - La Texture2D crée, null si le fichier n'existe pas</returns>
		public static async Task<Texture2D> ReadImageFileToTextureAsync(string fileName, StorageType storage,
			string subFolder = "")
		{
			return await ReadImageFileToTextureAsync(Path.Combine(storage.GetPath(), subFolder, fileName));
		}

		public static Texture2D ReadImageFileToTexture(string fileName, StorageType storage, string subFolder = "")
		{
			return ReadImageFileToTexture(Path.Combine(storage.GetPath(), subFolder, fileName));
		}

		#endregion Fichier vers Image Unity

		#region Image vers fichier

		#region Texture2D

		/// <summary>
		/// Écrit un fichier Image à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Texture2D texture, string fullPath,
			bool overwriteIfExist = true, int quality = 75, ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await Task.Run(async () =>
			{
				byte[] encodedTexture = null;
				switch (imageFileType)
				{
					case ImageFileType.JPG:
						encodedTexture = texture.EncodeToJPG(quality);
						break;

					case ImageFileType.PNG:
						encodedTexture = texture.EncodeToPNG();
						break;

					case ImageFileType.OpenEXR:
						encodedTexture =
							texture.EncodeToEXR(quality > 50
								? Texture2D.EXRFlags.OutputAsFloat
								: Texture2D.EXRFlags.None);
						break;

					case ImageFileType.TGA:
						encodedTexture = texture.EncodeToTGA();
						break;

					default:
						break;
				}

				return await WriteBytesAsync(encodedTexture, fullPath);
			});
		}

		/// <summary>
		/// Écrit un fichier Image à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">string - emplacement du fichier</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Texture2D texture, string fileName, string path,
			bool overwriteIfExist = true, int quality = 75, ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await WriteImageFile(texture, Path.Combine(GetFolderOrRoot(path), fileName), overwriteIfExist,
				quality, imageFileType);
		}

		/// <summary>
		/// Écrit un fichier Image à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Texture2D texture, string fileName, StorageType storage,
			string subFolder = "", bool overwriteIfExist = true, int quality = 75,
			ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await WriteImageFile(texture, Path.Combine(storage.GetPath(), subFolder, fileName), overwriteIfExist,
				quality, imageFileType);
		}

		#endregion Texture2D

		#region Sprite

		/// <summary>
		/// Écrit un fichier Image à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Sprite sprite, string fullPath, bool overwriteIfExist = true,
			int quality = 75, ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await WriteImageFile(sprite.GetTextureFromSprite(), fullPath, overwriteIfExist, quality,
				imageFileType);
		}

		/// <summary>
		/// Écrit un fichier Image à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">string - emplacement du fichier</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Sprite sprite, string fileName, string path,
			bool overwriteIfExist = true, int quality = 75, ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await WriteImageFile(sprite.GetTextureFromSprite(), Path.Combine(GetFolderOrRoot(path), fileName),
				overwriteIfExist, quality, imageFileType);
		}

		/// <summary>
		/// Écrit un fichier Image à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="quality">int - qualité de l'encodage pour le JPG, compression pour l'EXR (<50 = 16 bits, >50 = 32bits)</param>
		/// <param name="imageFileType">ImageFileType - Type de fichier</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFile(Sprite sprite, string fileName, StorageType storage,
			string subFolder = "", bool overwriteIfExist = true, int quality = 75,
			ImageFileType imageFileType = ImageFileType.JPG)
		{
			return await WriteImageFile(sprite.GetTextureFromSprite(),
				Path.Combine(storage.GetPath(), subFolder, fileName), overwriteIfExist, quality, imageFileType);
		}

		#endregion Sprite

		#region Format OpenEXR

		#region Texture2D

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Texture2D texture, string fullPath,
			Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteBytesAsync(texture.EncodeToEXR(exrFlags), fullPath);
		}

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="path">string - emplacement du fichier</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Texture2D texture, string fileName, string path,
			Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteImageFileOpenEXR(texture, Path.Combine(GetFolderOrRoot(path), fileName), exrFlags);
		}

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'une texture
		/// </summary>
		/// <param name="texture">Texture2D - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Texture2D texture, string fileName,
			StorageType storage, string subFolder = "", Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteImageFileOpenEXR(texture, Path.Combine(storage.GetPath(), subFolder, fileName),
				exrFlags);
		}

		#endregion Texture2D

		#region Sprite

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fullPath">string - chemin complet du fichier (dossier + fichier)</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Sprite sprite, string fullPath,
			Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteImageFileOpenEXR(sprite.GetTextureFromSprite(), fullPath, exrFlags);
		}

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">string - emplacement du fichier</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Sprite sprite, string fileName, string path,
			Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteImageFileOpenEXR(sprite, Path.Combine(GetFolderOrRoot(path), fileName), exrFlags);
		}

		/// <summary>
		/// Écrit un fichier Image au format OpenEXR à partir d'un Sprite
		/// </summary>
		/// <param name="sprite">Sprite - image source</param>
		/// <param name="fileName">String - nom voulu pour le fichier</param>
		/// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
		/// <param name="subFolder">String - sous-dossier</param>
		/// <param name="exrFlags">Texture2D.EXRFlags - Paramètre de compression</param>
		/// <returns>Renvoi false en cas d'erreur</returns>
		public static async Task<Exception> WriteImageFileOpenEXR(Sprite sprite, string fileName, StorageType storage,
			string subFolder = "", Texture2D.EXRFlags exrFlags = Texture2D.EXRFlags.None)
		{
			return await WriteImageFileOpenEXR(sprite, Path.Combine(storage.GetPath(), subFolder, fileName), exrFlags);
		}

		#endregion Sprite

		#endregion Format OpenEXR

		#endregion Image vers fichier

		public static (int width, int height) GetImageDimensions(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using (var reader = new BinaryReader(fs))
			{
				var header = reader.ReadBytes(8);
				if (IsPng(header))
				{
					return ReadPngDimensions(reader);
				}
				else if (IsJpeg(header))
				{
					return ReadJpegDimensions(reader);
				}
				else
				{
					return (0, 0);
				}
			}
		}

		private static bool IsPng(byte[] header)
		{
			return header[0] == 137 && header[1] == 80 && header[2] == 78 && header[3] == 71;
		}

		private static bool IsJpeg(byte[] header)
		{
			return header[0] == 0xFF && header[1] == 0xD8;
		}

		private static (int width, int height) ReadPngDimensions(BinaryReader reader)
		{
			reader.BaseStream.Seek(16, SeekOrigin.Begin);
			var width = ReadBigEndianInt32(reader);
			var height = ReadBigEndianInt32(reader);
			return (width, height);
		}

		private static (int width, int height) ReadJpegDimensions(BinaryReader reader)
		{
			reader.BaseStream.Seek(2, SeekOrigin.Begin); // Skip the initial SOI marker

			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				if (reader.ReadByte() == 0xFF)
				{
					var marker = reader.ReadByte();

					// Skip markers that are not SOF0 or SOF2
					if (marker == 0xC0 || marker == 0xC2)
					{
						reader.BaseStream.Seek(3, SeekOrigin.Current); // Skip length and precision
						int height = ReadBigEndianInt16(reader);
						int width = ReadBigEndianInt16(reader);
						return (width, height);
					}
					else
					{
						var length = ReadBigEndianInt16(reader);
						if (length < 2 || reader.BaseStream.Position + length - 2 > reader.BaseStream.Length)
						{
							return (0, 0);
						}

						reader.BaseStream.Seek(length - 2, SeekOrigin.Current);
					}
				}
			}

			return (0, 0);
		}

		private static int ReadBigEndianInt32(BinaryReader reader)
		{
			var bytes = reader.ReadBytes(4);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToInt32(bytes, 0);
		}

		private static short ReadBigEndianInt16(BinaryReader reader)
		{
			var bytes = reader.ReadBytes(2);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}

			return BitConverter.ToInt16(bytes, 0);
		}
	}
}