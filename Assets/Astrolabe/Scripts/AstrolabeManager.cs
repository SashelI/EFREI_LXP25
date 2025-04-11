using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Astrolabe.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Assets.Astrolabe.Scripts
{
	public class AstrolabeManager : MonoBehaviour
	{
		public AstrolabeEngine Engine { get; private set; }

		public static AstrolabeManager Instance
		{
			get => _instance;

			set
			{
				if (_instance == null)
				{
					_instance = value;
				}
			}
		}

		private static AstrolabeManager _instance;

		private void Awake()
		{
			Instance = this;
			Initialize();
		}

		/// <summary>
		/// Lancement externe de AstrolabeManager
		/// </summary>
		public static void InitializeApplication()
		{
			if (Instance != null)
			{
				Instance.Initialize();
			}
		}

		private void Initialize()
		{
			// Si TwinkleMaterialFactory ou TwinklePrefabFactory ne sont pas chargé avant on ne fait rien
			// Ce sont eux qui seront chargé de lancer l'execution

			if (TwinkleMaterialFactory.Instance == null || TwinklePrefabFactory.Instance == null ||
			    TwinkleDefaultSettings.Instance == null)
			{
				return;
			}

			Engine = new AstrolabeEngine();
			Engine.Initialize(transform);
		}

		// Update is called once per frame
		private void Update()
		{
			if (Engine != null)
			{
				Engine.Update();
			}
		}
	}
}