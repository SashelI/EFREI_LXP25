using System;
using Astrolabe.Twinkle;
#if ENABLE_WINMD_SUPPORT
using System.Threading.Tasks;
using Windows.System;
#endif

namespace Assets.Astrolabe.Scripts.Operations
{
	public class UnityLauncherOperations : ILauncherOperations
	{
#if ENABLE_WINMD_SUPPORT
public void LaunchUri(string uriString, Action<string, bool> completed)
    {
        Uri uri;

        try
        {
            uri = new Uri(uriString);
        }
        catch
        {
            completed?.Invoke(uriString, false);
            return;
        }

        bool result = false;

        UnityEngine.WSA.Application.InvokeOnUIThread( async () =>
        {
            result = await Launcher.LaunchUriAsync(uri);
        }, true);

        completed?.Invoke(uriString, result);        
    }
#else
		public void LaunchUri(string uriString, Action<string, bool> completed)
		{
			completed?.Invoke(uriString, true);
		}
#endif
	}
}