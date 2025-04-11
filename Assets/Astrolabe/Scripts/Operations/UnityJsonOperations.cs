using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Operations
{
	public class UnityJsonOperations : IJsonOperations
	{
		public T Deserialize<T>(string json)
		{
			return JsonUtility.FromJson<T>(json);
		}

		public string Serialize(object objectToSerialize)
		{
			var json = JsonUtility.ToJson(objectToSerialize, false);

			return json;
		}
	}
}