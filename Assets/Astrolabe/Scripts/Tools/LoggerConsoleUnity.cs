using Astrolabe.Diagnostics;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	public class LoggerConsoleUnity : LoggerBase
	{
		protected override void WriteLog(LogMessageType messageType, string message)
		{
			switch (messageType)
			{
				case LogMessageType.Information:
					Debug.Log(message);
					break;
				case LogMessageType.Warning:
					Debug.LogWarning(message);
					break;
				case LogMessageType.Error:
					Debug.LogError(message);
					break;
			}
		}
	}
}