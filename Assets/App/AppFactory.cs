using Assets.Astrolabe.Scripts;
using Astrolabe.Twinkle;
using UnityEngine;

namespace Assets.App
{
	public class AppFactory : MonoBehaviour, IApp
	{
		public LogicalApplication CreateApp()
		{
			return new App();
		}
	}
}
