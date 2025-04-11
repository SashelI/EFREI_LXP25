using System;
using Assets.Astrolabe.Scripts.MRTK_Enhanced;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;

namespace Assets.App
{
	public partial class App : LogicalApplication
	{
		protected override void InitializeComponent()
		{
			TwinkleApplication.Instance.Framework.Settings.AccentColor = new global::Astrolabe.Twinkle.Color(0x00, 0xA8, 0xFF);

			try
			{
				LoadComponent("App:///App.tml");
			}
			catch (Exception e)
			{
				Log.WriteLine(e);
			}

			// Disable gaze interactions
			InteractorBehaviorControls.Instance.DisableGaze();
		}
	}
}