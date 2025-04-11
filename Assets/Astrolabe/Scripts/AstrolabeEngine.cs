using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Components;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Operations.Hololens;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.IO;
using Astrolabe.Twinkle.Tools;
using UnityEngine;

namespace Assets.Astrolabe.Scripts
{
	public class AstrolabeEngine
	{
		private readonly XBoxControllerManager _xboxController = new();

		private readonly LogicalElement _rootElement = null;

		private string _designTwinkleString;
		private string _oldDesignTwinkleString;

		private LogicalApplication _application;

		private bool _isInitialized = false;

		public bool IsInitialized => _isInitialized;

		public string DesignTwinkleString => _designTwinkleString;

		public Transform Parent { get; private set; }

		public AstrolabeEngine()
		{
		}

		private DateTime _startAstrolabeAwake;

		public VoidObject VoidObject { get; private set; }

		public void Initialize(Transform parent)
		{
			Parent = parent;

			// Si TwinkleMaterialFactory ou TwinklePrefabFactory ne sont pas chargé avant on ne fait rien
			// Ce sont eux qui seront chargé de lancer l'execution

			if (TwinkleMaterialFactory.Instance == null || TwinklePrefabFactory.Instance == null ||
			    TwinkleDefaultSettings.Instance == null)
			{
				return;
			}

			_startAstrolabeAwake = DateTime.Now;

			// ici voidObject
			VoidObject = new VoidObject();
			// Creation du GameObject Void enfant de Astrolabe
			VoidObject.Initialize();

			StorageTypeExtension.Initialize();

			VisualReflector.LinkToVisualFactory(typeof(AstrolabeManager).Assembly);

			global::Astrolabe.Diagnostics.ILogger logger = new LoggerConsoleUnity();

			var framework = new TwinkleFramework(
				new UnityFileOperations(),
				new UnityJsonOperations(),
#if UNITY_EDITOR || WINDOWS_UWP
				new HololensMrtkQrCodeOperations(),
#else
				null,
#endif
				new UnityLauncherOperations(),

#if WINDOWS_UWP &&!UNITY_EDITOR
				new HololensPdfOperations(),
#elif UNITY_ANDROID && !UNITY_EDITOR
				new AndroidPdfOperations(),
#else
				new EditorPdfOperations(),
#endif

				new UnityKeyboardOperations(),
				new UnitySettingsOperations()
			);

			// Converter Twinkle
			var pixelConverter = new PixelConverter(50, 0.05f);
			var fontConverter = new PixelConverter(20, 0.04f);
			const float visualForwardStep = 0.0001f;

			// Settings de Twinkle
			framework.Settings.PixelConverter = pixelConverter;
			framework.Settings.FontConverter = fontConverter;
			framework.Settings.VisualForwardStep = visualForwardStep;

			var twinkleAssemblies = new List<Assembly>();

			TwinkleApplication.Instance.Initialize(framework, logger);

			if (TwinkleApplication.Instance.IsDesignMode)
			{
				InitializeDesignTime();
			}
			else
			{
				InitializeRunTime();
			}

			var durationAstrolabeAwake = DateTime.Now - _startAstrolabeAwake;

			Log.WriteLine("Astrolabe Version " + TwinkleApplication.Instance.Version);
			Log.WriteLine("DesigneMode=" + TwinkleApplication.Instance.IsDesignMode);

			_isInitialized = true;
		}

		private void InitializeRunTime()
		{
			var appFactory = GameObject.FindGameObjectWithTag("AstrolabePrefab")?.GetComponent<IApp>();

			if (appFactory == null)
			{
				throw new ArgumentNullException("appFactory",
					"ASTROLABE - App Init failed : please make sure the AppFactory script is on Astrolabe Prefab in the scene !");
			}

			var app = appFactory.CreateApp();
			CreateApplication(app);
		}

		private void InitializeDesignTime()
		{
			_designTwinkleString = "<App></App>";
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="relativeFilenameTml"></param>
		/// <returns></returns>
		public string GetDesignFullFilenameTml(string relativeFilenameTml)
		{
			return Path.Combine(TwinkleApplication.Instance.DesignPath, "Assets\\StreamingAssets\\App",
				relativeFilenameTml);
		}

		/// <summary>
		/// Chargement de TwinkleString en mode design (mode d'affichage instantané)
		/// </summary>
		/// <param name="fullFilenameTml"></param>
		/// <returns></returns>
		public async Task<bool> LoadDesignTwinkleStringAsync(string fullFilenameTml)
		{
			try
			{
#if WINDOWS_UWP
				if (await FileHelper.CheckBroadFileAccessAsync(fullFilenameTml) == false)
				{
					// Appel des settings de BroadAccessFileSystem pour accéder à l'entièreté du disque
					// fonctionnalité UWP
					UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
					{
						await FileHelper.LaunchBroadAccessFileSystemSettingsAsync();
					},
					true
					);
				}
#endif

				Log.WriteLine("Load " + fullFilenameTml);
				var result = await FileHelper.ReadAllTextAsync(fullFilenameTml);

				if (result.HasError == false)
				{
					_designTwinkleString = result.Content;

					TwinkleApplication.Instance.IsDesignMode = true;

					return true;
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);

				_designTwinkleString = null;
			}

			return false;
		}

		public bool IsLoading { get; private set; }

		private void UpdateFromString(string twinkleString)
		{
			if (twinkleString == null)
			{
				return;
			}

			if (IsLoading == true)
			{
				return;
			}

			IsLoading = true;

			try
			{
				// virer les anciens elements contenue dans void
				VoidObject.DisposeAllFromVoid();

				var oldRootElement = _rootElement;

				// En cas d'erreur le reste n'est pas executé
				var result = TmlReader.Load(twinkleString, false, true);

				if (result.HasError)
				{
					Log.WriteLine(result.Exception);
				}

				if (result.LogicalRoot != null)
				{
					var newElement = result.LogicalRoot as LogicalElement;

					CreateApplication(newElement);
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex);
			}
			finally
			{
				IsLoading = false;
			}
		}

		private void CreateApplication(LogicalElement newElement)
		{
			if (newElement != null)
			{
				// ici l'ancienne application est nettoyée puis recrée
				_application = TwinkleApplication.Instance.CreateApplicationBase(newElement);

				var twinkleComponent = _application.VisualElement.Element as TwinkleComponent;
				twinkleComponent.transform.SetParent(Parent, false);

				// Force le Update
				_application.ForceUpdateLayoutRoot();

				ApplicationCreated?.Invoke(_application, EventArgs.Empty);
			}
		}

		public event EventHandler ApplicationCreated;

		private bool _isUpdating = false;

		// Update is called once per frame
		public void Update()
		{
			if (_isInitialized)
			{
				// Storyboard
				TwinkleApplication.Instance.ExecuteRenderingEvent();

				// XBOX
				_xboxController.CheckStates();

				// Update
				if (TwinkleApplication.Instance.IsDesignMode == true)
				{
					// Mode DesignTime
					UpdateDesignTime();
				}
				else
				{
					// Mode Runtime
					UpdateRunTime();
				}
			}
		}

		private void UpdateRunTime()
		{
			TwinkleApplication.Instance.UpdateLayout();
		}

		private void UpdateDesignTime()
		{
			if (_isUpdating == false && _designTwinkleString != _oldDesignTwinkleString &&
			    string.IsNullOrWhiteSpace(_designTwinkleString) == false)
			{
				_isUpdating = true;

				try
				{
					UpdateFromString(_designTwinkleString);
				}
				finally
				{
					_oldDesignTwinkleString = _designTwinkleString;
					_isUpdating = false;
				}
			}
			else
			{
				TwinkleApplication.Instance.UpdateLayout();
			}
		}
	}

	public enum AstroLabLaunchMode
	{
		// in DesignMode
		Design,

		// in RunTime (default)
		RunTime
	}
}