using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations
{
	internal class UnitySettingsOperations : ISettingsOperations
	{
		public TwinkleSerializableSettings GetApplicationDefaultSerializableSettings()
		{
			return new TwinkleSerializableSettings
			{
				LightHouseIpAddress = TwinkleDefaultSettings.Instance.defaultLightHouseIpAddress,
				AzureSpeechRecognitionKey = TwinkleDefaultSettings.Instance.defaultAzureSpeechRecognitionKey,
				AzureSpeechRecognitionRegion = TwinkleDefaultSettings.Instance.defaultAzureSpeechRecognitionRegion,
				IgnoredLogTags = TwinkleDefaultSettings.Instance.ignoredLogTags
			};
		}
	}
}