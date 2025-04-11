using System;
using System.Collections;
using System.IO;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Astrolabe.Scripts.Tools
{
	/// <summary>
	/// Path management for SVG Astrolabe images.
	/// On Android, we cannot open a file stream (and thus open a svg with skia) inside streaming assets folder.
	/// So Foundation needs to move those files on build to keep Core workflow intact.
	/// </summary>
	public class UnityAndroidSourceManagerFactory : MonoBehaviour, IAndroidSourceManagerFactory
	{
		public IAndroidSourceManager Create()
		{
			return new AndroidSourceManager();
		}

		void Awake()
		{
			AndroidSourceManagerProvider.Factory = this;
		}

	}
}
