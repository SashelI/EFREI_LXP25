using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class VectorExtension
	{
		public static Vector3 ToVector3(this UnityEngine.Vector3 vector3)
		{
			return new Vector3(vector3.x, vector3.y, vector3.z);
		}

		public static Vector2 ToVector2(this UnityEngine.Vector2 vector2)
		{
			return new Vector2(vector2.x, vector2.y);
		}

		public static UnityEngine.Vector3 ToVector3(this Vector3 vector3)
		{
			return new UnityEngine.Vector3(vector3.X, vector3.Y, vector3.Z);
		}

		public static UnityEngine.Vector2 ToVector2(this Vector2 vector2)
		{
			return new UnityEngine.Vector2(vector2.X, vector2.Y);
		}
	}
}