using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class SpriteExtension
	{
		/// <summary>
		/// Crée une texture à partir du Sprite
		/// </summary>
		/// <param name="sprite"></param>
		/// <returns>Sprite</returns>
		public static Texture2D GetTextureFromSprite(this Sprite sprite)
		{
			try
			{
				if (sprite.rect.width != sprite.texture.width)
				{
					var newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
					var newColors = sprite.texture.GetPixels(Mathf.CeilToInt(sprite.textureRect.x),
						Mathf.CeilToInt(sprite.textureRect.y),
						Mathf.CeilToInt(sprite.textureRect.width),
						Mathf.CeilToInt(sprite.textureRect.height));
					newText.SetPixels(newColors);
					newText.Apply();
					return newText;
				}
				else
				{
					return sprite.texture;
				}
			}
			catch
			{
				return sprite.texture;
			}
		}
	}
}