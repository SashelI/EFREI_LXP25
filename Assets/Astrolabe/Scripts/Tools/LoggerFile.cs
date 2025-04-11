using System;
using System.IO;
using Assets.Astrolabe.Scripts.Operations;
using Astrolabe.Diagnostics;

namespace Assets.Astrolabe.Scripts.Tools
{
	public sealed class LoggerFile : LoggerConsoleUnity
	{
		private readonly string _fullFilename;

		public LoggerFile()
		{
			var now = DateTime.Now;
			var filename = $"log{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.txt";
			_fullFilename = new Uri($"Data:///{filename}").GetFullFilename(false);
		}

		protected override void WriteLog(LogMessageType messageType, string message)
		{
			var content = messageType + ":" + message + Environment.NewLine;

			base.WriteLog(messageType, message);

			File.AppendAllText(_fullFilename, content);
		}
	}
}