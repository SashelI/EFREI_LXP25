using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class RenderTextureExtension
	{
		public static void Clear(this RenderTexture renderTexture, Color color)
		{
			var currentRenderTexture = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true, true, color);
			RenderTexture.active = currentRenderTexture;
		}

		public static void Clear(this RenderTexture renderTexture)
		{
			Clear(renderTexture, Color.clear);
		}

		public static void Clear(this Texture texture)
		{
			var renderTextureTexture = texture as RenderTexture;

			if (renderTextureTexture != null)
			{
				Clear(renderTextureTexture);
			}
		}
	}
}