using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Astrolabe.Scripts
{
	public class TwinkleDefaultSettings : MonoBehaviour
	{
		public static TwinkleDefaultSettings Instance { get; set; }

		public void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			// Lancement de AstrolabeManager synchronisé avec AstrolabeManager
			AstrolabeManager.InitializeApplication();
		}

		[FormerlySerializedAs("DefaultLightHouseIpAddress")] public string defaultLightHouseIpAddress;
		[FormerlySerializedAs("DefaultAzureSpeechRecognitionKey")] public string defaultAzureSpeechRecognitionKey;
		[FormerlySerializedAs("DefaultAzureSpeechRecognitionRegion")] public string defaultAzureSpeechRecognitionRegion;
		[FormerlySerializedAs("IgnoredLogTags")] public List<string> ignoredLogTags;
	}
}