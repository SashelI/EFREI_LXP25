using System;
using Astrolabe.Diagnostics;
using UnityEngine;

namespace Assets.Astrolabe.Scripts
{
	public class WebCamComponent : MonoBehaviour
	{
		public int desiredWebCamWidth = 2271;
		public int desiredWebCamHeight = 1278;

		private WebCamTexture _webCamTexture;
		private Color32[] _colors;

		public bool IsStarted { get; private set; }

		public static void Attach(Transform transformWebCamComponent, Action<WebCamComponent> webCamStarting,
			Action<WebCamComponent> webCamStarted)
		{
			var webCamComponent = transformWebCamComponent.GetComponent<WebCamComponent>();

			if (webCamComponent == null)
			{
				webCamComponent = transformWebCamComponent.gameObject.AddComponent<WebCamComponent>();
			}

			// Start n'est pas passé, on l'attend via l'event Started
			if (webCamComponent.IsStarted == false)
			{
				EventHandler webCamStartingHandler = null;

				webCamStartingHandler = (s, e) =>
				{
					webCamStarting(webCamComponent);
					webCamComponent.Starting -= webCamStartingHandler;
				};

				webCamComponent.Starting += webCamStartingHandler;


				EventHandler webCamStartedHandler = null;

				webCamStartedHandler = (s, e) =>
				{
					webCamStarted(webCamComponent);
					webCamComponent.Started -= webCamStartedHandler;
				};

				webCamComponent.Started += webCamStartedHandler;
			}
			else
			{
				webCamStarting(webCamComponent);
				webCamComponent.Play();
				webCamStarted(webCamComponent);
			}
		}

		public static void Detach(Transform transformWebCamComponent)
		{
			var webCamComponent = transformWebCamComponent.GetComponent<WebCamComponent>();

			Detach(webCamComponent);
		}

		public static void Detach(WebCamComponent webCamComponent)
		{
			if (webCamComponent != null)
			{
				Destroy(webCamComponent);
			}
		}

		public void Start()
		{
			try
			{
				_webCamTexture = new WebCamTexture(desiredWebCamWidth, desiredWebCamHeight);
				_webCamTexture.requestedFPS = 60;

				var devices = GetWebCamDevices();

				if (devices == null || devices.Length == 0)
				{
					return;
				}

				// test
				//Log.WriteLine("Selected WebCamDevice=" + webCamTexture.deviceName);

				//foreach (var device in devices)
				//{
				//    Log.WriteLine("WebCamDevice=" + device.name);

				//    if (device.availableResolutions != null)
				//    {
				//        foreach (var resolution in device.availableResolutions)
				//        {
				//            Log.WriteLine("Resolutions=" + resolution.width + "x" + resolution.height + " Refresh Rate=" + resolution.refreshRate);
				//        }
				//    }
				//}

				if (SelectedWebCamDevice == null)
				{
					foreach (var device in devices)
					{
						if (device.name == _webCamTexture.deviceName)
						{
							_selectedWebCamDevice = device;
							break;
						}
					}
				}

				Starting?.Invoke(this, EventArgs.Empty);

				Play();

				_colors = new Color32[_webCamTexture.width * _webCamTexture.height];

				IsStarted = true;

				Started?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				IsStarted = false;
				Log.WriteLine(ex);
			}
		}

		// une fois la WebCam démarré
		public event EventHandler Started;

		// avant le demarrage de la WebCam
		public event EventHandler Starting;

		/// <summary>
		/// La propriété Frame doit^elle etre rafraichi ?
		/// Si l'evenenemt WebCamFrameUpdated est utilisé Frame est rafraichi automatiquement
		/// Il est dépendant de IsFrameLocked
		/// </summary>

		public bool RefreshFrame { get; set; } = true;

		/// <summary>
		/// La Frame n'est pas mise à jour à partir de la texture de la WebCam
		/// Prtaique dans le cas ou l'on doit faire un traitement long sur Frame ou dans l'event WebCamFrameUpdated
		/// </summary>

		public bool IsFrameLocked { get; set; } = false;

		public bool IsPlaying => _webCamTexture.isPlaying;

		public void Play()
		{
			if (_webCamTexture != null)
			{
				_webCamTexture.Stop();
				_webCamTexture.Play();
			}
		}

		public void Pause()
		{
			_webCamTexture?.Pause();
		}

		public void Stop()
		{
			// il y a un bug dans l'éditeur qui empeche la texture d'arreter la diffusion
			// est-ce corriger coté H2
			_webCamTexture?.Stop();
		}

		public WebCamDevice? SelectedWebCamDevice
		{
			get => _selectedWebCamDevice;

			set
			{
				_selectedWebCamDevice = value;

				SetSelectedWebCamDevice(value);
			}
		}

		private void SetSelectedWebCamDevice(WebCamDevice? value)
		{
			if (value != null)
			{
				var isPlaying = _webCamTexture.isPlaying;

				if (isPlaying)
				{
					_webCamTexture.Stop();
				}

				_webCamTexture.deviceName = value.Value.name;

				if (isPlaying)
				{
					Play();

					_colors = new Color32[_webCamTexture.width * _webCamTexture.height];
				}
			}
		}

		private WebCamDevice? _selectedWebCamDevice = null;

		public WebCamDevice[] GetWebCamDevices()
		{
			return WebCamTexture.devices;
		}

		public void AttachTexture(Transform transform)
		{
			if (IsStarted == false)
			{
				throw new Exception("You must start the WebCam before that you can attach it on a texture!");
			}

			;

			if (transform != null)
			{
				var renderer = transform.GetComponent<Renderer>();

				if (renderer != null && renderer.material != null)
				{
					renderer.material.mainTexture = _webCamTexture;
				}
			}
		}

		public int Width => _webCamTexture.width;

		public int Height => _webCamTexture.height;

		public Color32[] Frame => _colors;

		private byte[] CreateArray(int bytePerColor)
		{
			return new byte[_colors.Length * bytePerColor];
		}

		public byte[] CreateRgb24Array()
		{
			return CreateArray(3);
		}

		public byte[] CreateRgba32Array()
		{
			return CreateArray(4);
		}

		public void FrameToRgb24Array(byte[] rgbArray)
		{
			FrameToRgb24Array(_colors, rgbArray, Width, Height);
		}

		private static void FrameToRgb24Array(Color32[] colors, byte[] rgbArray, int widthWebCam, int heightWebCam)
		{
			var i = 0;

			for (var c = 0; c < colors.Length; c++)
			{
				var color = colors[c];

				rgbArray[i++] = color.r;
				rgbArray[i++] = color.g;
				rgbArray[i++] = color.b;
			}
		}

		public void FrameToRgba32Array(byte[] rgbaArray)
		{
			FrameToRgba32Array(_colors, rgbaArray, Width, Height);
		}

		private static void FrameToRgba32Array(Color32[] colors, byte[] rgbaArray, int widthWebCam, int heightWebCam)
		{
			var i = 0;

			for (var c = 0; c < colors.Length; c++)
			{
				var color = colors[c];

				rgbaArray[i++] = color.r;
				rgbaArray[i++] = color.g;
				rgbaArray[i++] = color.b;
				rgbaArray[i++] = color.a;
			}
		}

		// Update is called once per frame
		private void Update()
		{
			if (IsStarted == true && _webCamTexture.isPlaying == true)
			{
				if (WebCamFrameUpdated != null || RefreshFrame == true)
				{
					if (IsFrameLocked == false)
					{
						if (_webCamTexture.didUpdateThisFrame)
						{
							_webCamTexture.GetPixels32(_colors);

							WebCamFrameUpdated?.Invoke(this,
								new WebCamFrameUpdatedEventArgs(_colors, Width, Height)
							);
						}
					}
				}
			}
		}

		public event EventHandler WebCamFrameUpdated;

		// Argument permettant de recupérer la valeur

		public class WebCamFrameUpdatedEventArgs : EventArgs
		{
			public WebCamFrameUpdatedEventArgs(Color32[] frame, int width, int height)
			{
				Frame = frame;
				Width = width;
				Height = height;
			}

			public int Width { get; private set; }

			public int Height { get; private set; }

			public Color32[] Frame { get; private set; }

			public void ToRgb24Array(byte[] rgbArray)
			{
				FrameToRgb24Array(Frame, rgbArray, Width, Height);
			}

			public void ToRgba32Array(byte[] rgbaArray)
			{
				FrameToRgba32Array(Frame, rgbaArray, Width, Height);
			}
		}
	}
}