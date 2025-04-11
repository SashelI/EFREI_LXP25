/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Assets.Astrolabe.Libs.OBJImport.TextureLoader;
using Astrolabe.Diagnostics;
using UnityEngine;

namespace Assets.Astrolabe.Libs.OBJImport
{
	public class MTLLoader
	{
		public List<string> SearchPaths = new() { "%FileName%_Textures", string.Empty };

		private FileInfo _objFileInfo = null;

		/// <summary>
		/// The texture loading function. Overridable for stream loading purposes.
		/// </summary>
		/// <param name="path">The path supplied by the OBJ file, converted to OS path seperation</param>
		/// <param name="isNormalMap">Whether the loader is requesting we convert this into a normal map</param>
		/// <returns>Texture2D if found, or NULL if missing</returns>
		public virtual Texture2D TextureLoadFunction(string path, bool isNormalMap)
		{
			//find it
			foreach (var searchPath in SearchPaths)
			{
				//replace varaibles and combine path
				var processedPath = _objFileInfo != null
					? searchPath.Replace("%FileName%", Path.GetFileNameWithoutExtension(_objFileInfo.Name))
					: searchPath;
				var filePath = Path.Combine(processedPath, path);

				//return if eists
				if (File.Exists(filePath))
				{
					var tex = ImageLoader.LoadTexture(filePath);

					if (isNormalMap)
					{
						tex = ImageUtils.ConvertToNormalMap(tex);
					}

					return tex;
				}
			}

			//not found
			return null;
		}

		private Texture2D TryLoadTexture(string texturePath, bool normalMap = false)
		{
			//swap directory seperator char
			texturePath = texturePath.Replace('\\', Path.DirectorySeparatorChar);
			texturePath = texturePath.Replace('/', Path.DirectorySeparatorChar);

			return TextureLoadFunction(texturePath, normalMap);
		}

		private int GetArgValueCount(string arg)
		{
			switch (arg)
			{
				case "-bm":
				case "-clamp":
				case "-blendu":
				case "-blendv":
				case "-imfchan":
				case "-texres":
					return 1;

				case "-mm":
					return 2;

				case "-o":
				case "-s":
				case "-t":
					return 3;
			}

			return -1;
		}

		private int GetTexNameIndex(string[] components)
		{
			for (var i = 1; i < components.Length; i++)
			{
				var cmpSkip = GetArgValueCount(components[i]);
				if (cmpSkip < 0)
				{
					return i;
				}

				i += cmpSkip;
			}

			return -1;
		}

		private float GetArgValue(string[] components, string arg, float fallback = 1f)
		{
			var argLower = arg.ToLower();
			for (var i = 1; i < components.Length - 1; i++)
			{
				var cmp = components[i].ToLower();
				if (argLower == cmp)
				{
					return OBJLoaderHelper.FastFloatParse(components[i + 1]);
				}
			}

			return fallback;
		}

		private string GetTexPathFromMapStatement(string processedLine, string[] splitLine)
		{
			var texNameCmpIdx = GetTexNameIndex(splitLine);
			if (texNameCmpIdx < 0)
			{
				Log.WriteLine($"texNameCmpIdx < 0 on line {processedLine}. Texture not loaded.", LogMessageType.Error);
				return null;
			}

			var texNameIdx = processedLine.IndexOf(splitLine[texNameCmpIdx]);
			var texturePath = processedLine.Substring(texNameIdx);

			return texturePath;
		}

		/// <summary>
		/// Loads a *.mtl file
		/// </summary>
		/// <param name="input">The input stream from the MTL file</param>
		/// <returns>Dictionary containing loaded materials</returns>
		public Dictionary<string, Material> Load(Stream input, global::Astrolabe.Twinkle.ShaderMode shaderMode)
		{
			Dictionary<string, Material> mtlDict = null;

			try
			{
				var inputReader = new StreamReader(input);
				var reader = new StringReader(inputReader.ReadToEnd());

				mtlDict = new Dictionary<string, Material>();
				Material currentMaterial = null;

				for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
				{
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					var processedLine = line.Clean();
					var splitLine = processedLine.Split(' ');

					//blank or comment
					if (splitLine.Length < 2 || processedLine[0] == '#')
					{
						continue;
					}

					var command = splitLine[0];

					//newmtl
					if (command == "newmtl")
					{
						var materialName = processedLine.Substring(7);

						var newMtl = OBJLoaderHelper.CreateDefaultMaterial(shaderMode, materialName);

						mtlDict[materialName] = newMtl;
						currentMaterial = newMtl;

						continue;
					}

					//anything past here requires a material instance
					if (currentMaterial == null)
					{
						continue;
					}

					switch (command)
					{
						//diffuse color
						case "Kd":
						case "kd":

							var currentColor = currentMaterial.GetColor("_Color");
							var kdColor = OBJLoaderHelper.ColorFromStrArray(splitLine);

							currentMaterial.SetColor("_Color", new Color(kdColor.r, kdColor.g, kdColor.b, currentColor.a));
							break;

						//diffuse map
						case "map_Kd":
						case "map_kd":

							var texturePath = GetTexPathFromMapStatement(processedLine, splitLine);

							if (texturePath == null)
							{
								break; //invalid args or sth
							}

							var KdTexture = TryLoadTexture(texturePath);

							currentMaterial.SetTexture("_MainTex", KdTexture);

							//set transparent mode if the texture has transparency
							if (KdTexture != null && (KdTexture.format == TextureFormat.DXT5 ||
							                          KdTexture.format == TextureFormat.ARGB32))
							{
								// TODO: A changer pour le MRTK car le rendering mode transparency est mal fixé. Pour l'instant on vire
								//OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
							}

							//flip texture if this is a dds
							if (Path.GetExtension(texturePath).ToLower() == ".dds")
							{
								currentMaterial.mainTextureScale = new Vector2(1f, -1f);
							}

							break;
							;

						//bump map
						case "map_Bump":
						case "map_bump":

							texturePath = GetTexPathFromMapStatement(processedLine, splitLine);
							if (texturePath == null)
							{
								break; //invalid args or sth
							}

							var bumpTexture = TryLoadTexture(texturePath, true);
							var bumpScale = GetArgValue(splitLine, "-bm", 1.0f);
							if (bumpTexture != null)
							{
								currentMaterial.SetTexture("_BumpMap", bumpTexture);
								currentMaterial.SetFloat("_BumpScale", bumpScale);
								currentMaterial.EnableKeyword("_NORMALMAP");
							}

							break;

						//specular color
						case "Ks":
						case "ks":
							currentMaterial.SetColor("_SpecColor", OBJLoaderHelper.ColorFromStrArray(splitLine));
							break;

						//emission color
						case "Ka":
						case "ka":
							currentMaterial.SetColor("_EmissionColor", OBJLoaderHelper.ColorFromStrArray(splitLine, 0.05f));
							currentMaterial.EnableKeyword("_EMISSION");
							break;

						//emission map
						case "map_Ka":
						case "map_ka":

							texturePath = GetTexPathFromMapStatement(processedLine, splitLine);

							if (texturePath == null)
							{
								break; //invalid args or sth
							}

							currentMaterial.SetTexture("_EmissionMap", TryLoadTexture(texturePath));
							break;

						//alpha
						case "d":
						case "Tr":

							var visibility = OBJLoaderHelper.FastFloatParse(splitLine[1]);

							//tr statement is just d inverted
							if (command == "Tr")
							{
								visibility = 1f - visibility;
							}

							if (visibility < 1f - Mathf.Epsilon)
							{
								currentColor = currentMaterial.GetColor("_Color");

								currentColor.a = visibility;
								currentMaterial.SetColor("_Color", currentColor);

								// TODO modifier ou virer pour MRTK car deja en transparence
								//OBJLoaderHelper.EnableMaterialTransparency(currentMaterial);
							}

							break;
							;

						//glossiness
						case "Ns":
						case "ns":

							var Ns = OBJLoaderHelper.FastFloatParse(splitLine[1]);
							Ns = Ns / 1000f;
							currentMaterial.SetFloat("_Glossiness", Ns);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
				Log.WriteLine("Stack=" + ex.StackTrace);
			}

			//return our dict
			return mtlDict;
		}

		/// <summary>
		/// Loads a *.mtl file
		/// </summary>
		/// <param name="path">The path to the MTL file</param>
		/// <returns>Dictionary containing loaded materials</returns>
		public Dictionary<string, Material> Load(string path, global::Astrolabe.Twinkle.ShaderMode shaderMode)
		{
			_objFileInfo = new FileInfo(path); //get file info
			SearchPaths.Add(_objFileInfo.Directory.FullName); //add root path to search dir

			/*
        using (var fs = new FileStream(path, FileMode.Open))
        {
            return Load(fs); //actually load
        }
        */

			var objBytes = File.ReadAllBytes(path);

			using (var stream = new MemoryStream(objBytes))
			{
				return Load(stream, shaderMode);
			}
		}
	}
}