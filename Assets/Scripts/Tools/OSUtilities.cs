using System.Collections.Generic;
using UnityEngine;
#if WINDOWS_UWP
using Astrolabe.Diagnostics;
using System;
using Windows.System;
using Windows.Storage;
#endif

namespace Assets.Scripts.Tools
{
	public static class OSUtilities
	{
		/// <summary>
		/// Url vers la page du site web de synergiz pour les upsells
		/// Garder l'utm pour le tracking
		/// </summary>
		public static string UrlUpsell = @"https://www.welcometoharbor.io?utm_source=MetaQuest&utm_medium=Horizon&utm_id=HarborMQ";

		/// <summary>
		/// Launch Edge browser on url page
		/// </summary>
		/// <param name="url">Web url to open</param>
		/// <param name="useHoloBrowser"></param>
		public static void LaunchEdge(string url, bool useHoloBrowser = false)
		{
#if WINDOWS_UWP
        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            try
            {
                var uri = new Uri(url);
                if (useHoloBrowser)
                {
                    var holoBrowserUri = new Uri($"holo-browser:{url}");
                    var option = new LauncherOptions()
                    {
                        FallbackUri = uri
                    };
                    await Launcher.LaunchUriAsync(holoBrowserUri, option);
                }
                else
                {
                    await Launcher.LaunchUriAsync(uri);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(e, LogTags.MVVM_APP);
            }
        }, false);
#else
			Application.OpenURL(url);
#endif
		}
	}
}