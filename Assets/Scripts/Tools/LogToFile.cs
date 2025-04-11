#if UNITY_ANDROID && !UNITY_EDITOR
using System.IO;
using Harbor.Tools.Helpers;
#endif
using UnityEngine;

namespace Assets.Scripts.Tools
{
	public class LogToFile : MonoBehaviour
	{
#if UNITY_ANDROID && !UNITY_EDITOR
	private StreamWriter _logStreamWriter;

	private const string LOG_FILENAME = "logs.txt";

	void Awake()
	{
		var logFilePath = Path.Combine(StorageHelper.Instance.PersistentDataPath, LOG_FILENAME);

		_logStreamWriter = new StreamWriter(logFilePath, false);

		Application.logMessageReceived += LogMessage;
	}

	void OnDestroy()
	{
		Application.logMessageReceived -= LogMessage;
		_logStreamWriter.Close();
	}

	void LogMessage(string logString, string stackTrace, LogType type)
	{
		_logStreamWriter.WriteLine($"{System.DateTime.Now} [{type}] {logString}\n{stackTrace}");
		_logStreamWriter.Flush();
	}
#endif
	}
}
