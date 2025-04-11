using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class GameObjectExtension
	{
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			var component = gameObject.GetComponent<T>();

			if (component == null)
			{
				return gameObject.AddComponent<T>();
			}

			return component;
		}

		public static GameObject FindRecursively(this GameObject gameObject, string name)
		{
			return FindRecursively(gameObject.transform, name).gameObject;
		}

		public static Transform FindRecursively(this Transform transform, string name)
		{
			foreach (Transform child in transform)
			{
				if (child.name == name)
				{
					return child;
				}

				var result = child.FindRecursively(name);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		public static GameObject GetRoot(this GameObject gameObject)
		{
			return GetRoot(gameObject?.transform)?.gameObject;
		}

		public static Transform GetRoot(this Transform transform)
		{
			if (transform == null)
			{
				return null;
			}

			while (true)
			{
				if (transform.parent == null)
				{
					return transform;
				}

				transform = transform.parent;
			}
		}
	}
}