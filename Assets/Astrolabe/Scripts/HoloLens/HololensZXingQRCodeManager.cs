using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if WINDOWS_UWP
using ZXing;
using ZXing.Common;
using ZXing.QrCode;


public class HololensZXingQRCodeManager
{
	private Transform transformWebCamComponent;
	private byte[] rgbArray;

	//il existe aussi BarcodeReader qui reconnait egalement les QRCodes
	private QRCodeReader QRCodeReader = new QRCodeReader();

	public HololensZXingQRCodeManager()
	{
		var astrolabeGO = AstrolabeManager.Instance.gameObject;

		this.transformWebCamComponent = astrolabeGO.transform;
	}

	public WebCamComponent WebCam
	{
		get;
		private set;
	}

	public bool IsStarting
	{
		get;
		private set;
	}

	public bool IsScanning
	{
		get
		{
			return this.isScanning;
		}

		private set
		{
			this.isScanning = value;
			// si l'on scanne on lock la frame pour qu'il n'y ait plus de nouvelle texture
			// cela devrait soulager le CPU pendant le traitement

			if (this.WebCam != null)
			{
				this.WebCam.IsFrameLocked = value;
			}
		}
	}

	private bool isScanning = false;

	// class d'annulation pour DetectOnce

	public class DetectOnceCancellationSource : ICancellationSource
	{
		public DetectOnceCancellationSource(HololensZXingQRCodeManager qrCodeManager)
		{
			this.QRCodeManager = qrCodeManager;
		}

		public HololensZXingQRCodeManager QRCodeManager
		{
			get;
			private set;
		}

		public bool IsCancelling
		{
			get;
			private set;
		}

		public bool IsCancelled
		{
			get;
			private set;
		}

		public bool Cancel()
		{
			this.IsCancelling = true;

			if (this.QRCodeManager != null && this.QRCodeManager.IsStarting == true)
			{
				this.QRCodeManager.Stop();
				this.IsCancelled = true;

				return true;
			}

			return false;
		}
	}

	private async Task<Exception> EmulationForEditorAsync(Action qrCodeDetectionLaunch, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
	{
		Log.WriteLine("QRCodReader not suported! Emulation for editor");

		// Lancement de la detection
		qrCodeDetectionLaunch?.Invoke();

		await Task.Delay(3000);

		// Test Exception
		//var exception = new Exception("Test exception QRCode!");
		//qrCodeException(exception);
		//return exception;

		// test Cancellation
		//qrCodeDetected(null);

		var qrCodeContent = new QrCodeContent()
		{
			Id = Guid.NewGuid(),
			Data = "EL4894101", //"EL896503"
			HaveSpatialLocation = true,
			// QrCodeContent appartient à Logical alors on laisse les infos en Logical
			SpatialPosition = new UnityEngine.Vector3(0, 0, 0.8f).ToVector3().ToLogical(),
			SpatialRotation = new Astrolabe.Twinkle.Vector3(90, 0, 0),
			SpatialSize = (0.1f).ToLogical()
		};

		// test QRCode lu
		qrCodeDetected(qrCodeContent);

		return null;
	}

	public async Task<Exception> DetectOnceAsync(DetectOnceCancellationSource cancellationSource, Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
	{
		if (this.IsStarting == true)
		{
			return null;
		}

		//#if UNITY_EDITOR
		//return await this.EmulationForEditorAsync(qrCodeDetectionStarted, qrCodeDetected, qrCodeException);
		//#endif

		TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();

		Action<QrCodeContent> handlerQRCodeDetected = (qrCodeContent) =>
		{
			taskCompletionSource.TrySetResult(null);
			qrCodeDetected?.Invoke(qrCodeContent);
		};

		try
		{
			DetectOnce(qrCodeDetectionStarted, handlerQRCodeDetected, true);
		}
		catch (Exception ex)
		{
			return ex;
		}

		try
		{
			await taskCompletionSource.Task;
		}
		catch (Exception ex)
		{
			return ex;
		}

		return null;
	}

	private QrCodeContent qrCodeContent = new QrCodeContent();
	private Dictionary<string, Guid> qrCodeDataByGuid = new Dictionary<string, Guid>();

	private DetectOnceCancellationSource DetectOnce(Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, bool destroyCamera
 = true)
	{
		if (this.IsStarting == true)
		{
			return null;
		}

		DetectOnceCancellationSource cancellationSource = new DetectOnceCancellationSource(this);

		this.Start(
		 // Starting
		 null,

		// Started
		(WebCam) =>
		{
			// ici peut être que l'on a déjà annulé mais comme c'etait pas démarré on ne pouvait pas stopper
			if (cancellationSource.IsCancelling == true)
			{
				cancellationSource.Cancel();
				return;
			}

			EventHandler qrCodeDetectedHandler = null;

			qrCodeDetectedHandler = (s, e) =>
			{
				this.Stop(destroyCamera);

				var args = (QRCodeDetectedEventArgs)e;

				qrCodeContent.Data = args.Text;

				// normalement il faudrait prendre en compte la spatialisation
				if (qrCodeDataByGuid.ContainsKey(args.Text) == false)
				{
					qrCodeContent.Id = Guid.NewGuid();
					qrCodeDataByGuid.Add(args.Text, qrCodeContent.Id);
				}
				else
				{
					qrCodeContent.Id = qrCodeDataByGuid[args.Text];
				}

				qrCodeDetected?.Invoke(qrCodeContent);

				this.QRCodeDetected -= qrCodeDetectedHandler;
			};

			this.QRCodeDetected += qrCodeDetectedHandler;
		});

		qrCodeDetectionStarted?.Invoke();

		return cancellationSource;
	}

	// Start is called before the first frame update
	public void Start(Action<WebCamComponent> starting = null, Action<WebCamComponent> started = null)
	{
		if (this.IsStarting == false)
		{
			try
			{
				WebCamComponent.Attach(this.transformWebCamComponent,

				// avant le demarrage de la cam (par exemple selection de la WebCam)
				(webCamComponent) =>
				{
					this.WebCam = webCamComponent;
					starting?.Invoke(webCamComponent);
				},

				// après le démarrage de la cam
				(webCamComponent) =>
				{
					try
					{
						if (webCamComponent != null)
						{
							rgbArray = webCamComponent.CreateRgb24Array();
							webCamComponent.WebCamFrameUpdated += WebCamFrameUpdated;
							this.IsStarting = true;

							started?.Invoke(webCamComponent);

							return;
						}
					}
					catch (Exception ex)
					{
						Log.WriteLine(ex);
					}

					this.IsStarting = false;
				});
			}
			catch (Exception ex)
			{
				this.IsStarting = false;
				Log.WriteLine(ex);
			}
		}
	}

	public void Stop(bool destroyCamera = false)
	{
		if (this.IsStarting == true)
		{
			this.WebCam.WebCamFrameUpdated -= WebCamFrameUpdated;

			this.WebCam.Stop();

			if (destroyCamera == true)
			{
				GameObject.Destroy(this.WebCam);
			}

			this.WebCam = null;
			rgbArray = null;

			this.IsStarting = false;
		}
	}

	public string Text
	{
		get;
		private set;
	}

	public string LastTextRead
	{
		get;
		private set;
	}

	public bool WorkOnThreadPool
	{
		get;
		set;
	} = true;

	private bool workOnThreadPoolFreezed = false;
	private QRCodeDetectedEventArgs threadPoolResult;

	// Update is called once per frame
	private void WebCamFrameUpdated(object sender, EventArgs e)
	{
		if (this.IsStarting == false)
		{
			return;
		}

		// ici on recupère la valeur du QRCode géré dans le ThreadPool
		// C'est un dispatcher primitif mais très rapide

		if (workOnThreadPoolFreezed == true && threadPoolResult != null)
		{
			var result = this.threadPoolResult;
			this.threadPoolResult = null;
			QRCodeDetected?.Invoke(this, result);
			return;
		}

		if (this.IsScanning == true)
		{
			return;
		}

		this.IsScanning = true;

		var webCamFrameEventArgs = e as WebCamComponent.WebCamFrameUpdatedEventArgs;

		if (WorkOnThreadPool)
		{
			workOnThreadPoolFreezed = WorkOnThreadPool;

			bool result = false;

			try
			{
				// Test avec Thread
				//this.threadQRCodeDetection = new Thread((state) =>
				//{
				//    DetectQrCodeInFrame((WebCamComponent.WebCamFrameUpdatedEventArgs)state);
				//});
				//threadQRCodeDetection.Start(webCamFrameEventArgs);

				result = ThreadPool.QueueUserWorkItem((state) =>
				{
					DetectQrCodeInFrame((WebCamComponent.WebCamFrameUpdatedEventArgs)state);
				}, webCamFrameEventArgs);
			}
			// il n'a pas réussi à placer en queue == NotSupportedException
			catch
			{
				result = false;
			}

			if (result == false)
			{
				this.IsScanning = false;
			}
		}
		else
		{
			DetectQrCodeInFrame(webCamFrameEventArgs);
		}
	}

	private void DetectQrCodeInFrame(WebCamComponent.WebCamFrameUpdatedEventArgs webCamFrameEventArgs)
	{
		try
		{
			this.Text = null;

			int w = webCamFrameEventArgs.Width;
			int h = webCamFrameEventArgs.Height;

			webCamFrameEventArgs.ToRgb24Array(rgbArray);

			var eventArgs = new FrameReadEventArgs(rgbArray, w, h);

			// pour Cancellation
			if (this.IsStarting == false)
			{
				return;
			}

			FrameRead?.Invoke(this, eventArgs);

			byte[] changedFrame = rgbArray;

			if (eventArgs.FrameChanged)
			{
				changedFrame = eventArgs.FrameRgb;
			}

			LuminanceSource source = new RGBLuminanceSource(changedFrame, w, h);
			BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));

			Result result = QRCodeReader.decode(bitmap);

			if (result != null && result.Text != null)
			{
				this.Text = result.Text;
				this.LastTextRead = result.Text;

				// pour Cancellation
				if (this.IsStarting == false)
				{
					return;
				}

				// le resultat ne sera pas traité sur le ThreadPool pour éviter les problèmes de main thread

				threadPoolResult = new QRCodeDetectedEventArgs(result.Text, result.ResultPoints, changedFrame, w, h);
			}
		}
		catch (Exception ex)
		{
			Log.WriteLine(ex);
		}

		this.IsScanning = false;
	}

	public event EventHandler FrameRead;

	public event EventHandler QRCodeDetected;

	public class FrameReadEventArgs : EventArgs
	{
		public FrameReadEventArgs(byte[] rgbarray, int width, int height)
		{
			this.FrameRgb = rgbarray;
			this.Width = width;
			this.Height = height;
		}

		public string Text
		{
			get;
			private set;
		}

		// Format RGB (3 octets)
		public byte[] FrameRgb
		{
			get;
			set;
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public bool FrameChanged
		{
			get;
			set;
		}
	}

	public class QRCodeDetectedEventArgs : EventArgs
	{
		public QRCodeDetectedEventArgs(string text, ResultPoint[] zone, byte[] rgbarray, int width, int height)
		{
			this.Text = text;
			this.Zone = zone;
			this.Frame = rgbarray;
			this.Width = width;
			this.Height = height;
		}

		public string Text
		{
			get;
			private set;
		}

		public ResultPoint[] Zone
		{
			get;
			set;
		}

		// Format RGB (3 octets)
		public byte[] Frame
		{
			get;
			set;
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}
	}
}
#endif