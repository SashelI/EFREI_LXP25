using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class BrushExtension
	{
		public static UnityEngine.Color ToUnityColor(this SolidColorBrush brush)
		{
			var b = (SolidColorBrush)brush;
			var c = b.Color;

			return new UnityEngine.Color((float)c.R / 255f, (float)c.G / 255f, (float)c.B / 255f, (float)c.A / 255f);
		}

		public static UnityEngine.Color ToUnityColor(this Color color)
		{
			return new UnityEngine.Color((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f,
				(float)color.A / 255f);
		}

		/// <summary>
		/// On prend la couleur et son alpha et l'on multiplie l'alpha par l'opacity puis on renvoie la nouvelle couleur
		/// </summary>
		/// <param name="color"></param>
		/// <param name="opacity"></param>
		/// <returns></returns>
		public static UnityEngine.Color GetColorWithOpacity(this UnityEngine.Color color, float opacity)
		{
			if (opacity < 0f)
			{
				opacity = 0f;
			}

			if (opacity > 1f)
			{
				opacity = 1f;
			}

			return new UnityEngine.Color(color.r, color.g, color.b, color.a * opacity);
		}

		public static UnityEngine.Color32 GetColorWithOpacity(this UnityEngine.Color32 color, float opacity)
		{
			if (opacity < 0f)
			{
				opacity = 0f;
			}

			if (opacity > 1f)
			{
				opacity = 1f;
			}

			var a = (float)color.a * opacity;

			return new UnityEngine.Color32(color.r, color.g, color.b, (byte)a);
		}
	}
}