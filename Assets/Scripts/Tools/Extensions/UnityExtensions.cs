using UnityEngine;

namespace Assets.Scripts.Tools.Extensions
{
	public static class UnityExtensions 
	{
		public static Vector3Int ConvertToVector3(this Vector3 vec3)
		{
			return new Vector3Int((int)vec3.x, (int)vec3.y, (int)vec3.z);
		}

		public static void ResetTransformation(this Transform trans)
		{
			trans.position = Vector3.zero;
			trans.localRotation = Quaternion.identity;
			trans.localScale = new Vector3(1, 1, 1);
		}

		public static Vector2 Rotate(this Vector2 vector, float degrees)
		{
			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = vector.x;
			float ty = vector.y;
			vector.x = (cos * tx) - (sin * ty);
			vector.y = (sin * tx) + (cos * ty);
			return vector;
		}

		public static float RotationNormalizedDeg(this float rotation)
		{
			rotation = rotation % 360f;
			if (rotation < 0)
				rotation += 360f;
			return rotation;
		}

		public static void SetLayerRecursively(this GameObject gameObject, int layer)
		{
			gameObject.layer = layer;
			foreach (Transform t in gameObject.transform)
				t.gameObject.SetLayerRecursively(layer);
		}
		public static void DestroyChildren(this GameObject parent)
		{
			Transform[] children = new Transform[parent.transform.childCount];
			for (int i = 0; i < parent.transform.childCount; i++)
				children[i] = parent.transform.GetChild(i);
			for (int i = 0; i < children.Length; i++)
				Object.Destroy(children[i].gameObject);
		}
		public static void MoveChildren(this GameObject from, GameObject to)
		{
			Transform[] children = new Transform[from.transform.childCount];
			for (int i = 0; i < from.transform.childCount; i++)
				children[i] = from.transform.GetChild(i);
			for (int i = 0; i < children.Length; i++)
				children[i].SetParent(to.transform);
		}
		public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
		{
			var planes = GeometryUtility.CalculateFrustumPlanes(camera);
			return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		}
		public static bool Intersects(this Rect source, Rect rect)
		{
			return !((source.x > rect.xMax) || (source.xMax < rect.x) || (source.y > rect.yMax) || (source.yMax < rect.y));
		}
	}
}
