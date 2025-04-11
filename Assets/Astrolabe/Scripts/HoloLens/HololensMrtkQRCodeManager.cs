using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
using Microsoft.MixedReality.QR;

namespace Assets.Astrolabe.Scripts.HoloLens
{
	public class HololensMrtkQrCodeManager
	{
		//Lecteur de QRCodes du MRTK pour Hololens
#if UNITY_EDITOR || WINDOWS_UWP
		private QRCodeWatcher _qrCodeWatcher = null;
#endif

		public HololensMrtkQrCodeManager()
		{
		}

		private void TriggerQrCodeFound(QrCodeEventArgs args)
		{
			var code = args.QrCode;

			// on autorise le QRCode a etre detecté si :

			if (
				// seul la data est demandé (c'est forcement le cas si on est là)
				DetectionQrCodeType == DetectionQrCodeTypes.DataOnly
				||
				// la position est forcement demandée et est presente dans le QRCode
				(DetectionQrCodeType == DetectionQrCodeTypes.DataAndSpatialLocation && code.HaveSpatialLocation == true))
			{
				_detectOnceStopCallback(code);
				QrCodeFound?.Invoke(this, args);
			}
		}

		private void OnQRCodeWatcherUpdated(object sender,
#if UNITY_EDITOR || WINDOWS_UWP
			QRCodeUpdatedEventArgs e)
#else
		EventArgs e)
#endif
		{
			// le QRCode ne peut être sortie de l'event (il sera libéré)
			// on le clone dans un code ThreadSafe
#if UNITY_EDITOR || WINDOWS_UWP
			var threadSafeCode = e.Code.ToThreadSafeQrCode();
#endif
			TwinkleApplication.Instance.Dispatcher.Run(() =>
			{
				// on transform les coordonnées pour être utilisables par Unity+Astrolabe
#if UNITY_EDITOR || WINDOWS_UWP
				var code = threadSafeCode.ToQrCodeContent();

				var args = new QrCodeEventArgs(code);
#else
			QrCodeEventArgs args = null;
#endif
				QrCodeUpdated?.Invoke(sender, args);

				TriggerQrCodeFound(args);
			});
		}

		public bool IsStarted { get; private set; }

		// CallBack du detectOnce permettant un arrete depuis le jeton de Cancel
		private Action<QrCodeContent> _detectOnceStopCallback;

		/// <summary>
		/// Permet de savoir si l'on souhaite la lspatial location egalement
		/// </summary>

		public DetectionQrCodeTypes DetectionQrCodeType { get; private set; }

		public async Task<Exception> DetectOnceAsync(DetectOnceCancellationSource cancellationSource,
			Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException,
			DetectionQrCodeTypes detectionQrCodeType = DetectionQrCodeTypes.DataAndSpatialLocation)
		{
			if (IsStarted == true)
			{
				return null;
			}

#if UNITY_EDITOR
			return await EmulationForEditorAsync(cancellationSource, qrCodeDetectionStarted, qrCodeDetected,
				qrCodeException);
#elif WINDOWS_UWP
// pour la Cancellation ou l'event QRCodeFound
		this._detectOnceStopCallback = (qrCodeContent) =>
		{
			// Cela doit fonctionner une fois
			this.Stop();

			// on appelle le callback
			qrCodeDetected(qrCodeContent);
		};

		// On demande quel data dans le QRCode (Data+SpatialLocation?)
		this.DetectionQrCodeType = detectionQrCodeType;

		// On ajoute l'abonnement tout de suite pour être sur de ne rien perdre.
		//this.QRCodeFound += OnQRCodeFoundForDetectOnce;

		Log.WriteLine("StartAsync");
		var exception = await this.StartAsync(qrCodeDetectionStarted);

		Log.WriteLine("StartAsync Exception=" + exception);

		if (exception != null)
		{
			// Problème ! On retire l'abonnement
			// dans le cas ou c'est démarré il sera retiré plus tard
			//this.QRCodeFound -= OnQRCodeFoundForDetectOnce;

			// appel du callback
			qrCodeException?.Invoke(exception);
		}

		return exception;
#else
	return null;
#endif
		}


		private async Task<Exception> EmulationForEditorAsync(DetectOnceCancellationSource cancellationSource,
			Action qrCodeDetectionLaunch, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
		{
			IsStarted = true;

			Log.WriteLine("QRCodeWatcher not suported! Emulation for editor");

			// Lancement de la detection
			qrCodeDetectionLaunch?.Invoke();

			await Task.Delay(3000);

			if (cancellationSource.IsCanceled == true)
			{
				IsStarted = false;
				return null;
			}

			var qrCodeContent = new QrCodeContent()
			{
				Id = Guid.NewGuid(),
				Data = EmulationQrCodeData, // "https://www.kaspersky.com", // "EL4894101", //"EL896503"
				HaveSpatialLocation = true,
				// QRCodeContent appartient à Logical alors on laisse les infos en Logical
				SpatialPosition = new UnityEngine.Vector3(0, 0, 0.8f).ToVector3().ToLogical(),
				SpatialRotation = new Vector3(0, 0, 0),
				SpatialSize = 100f
			};

			// test QRCode lu
			qrCodeDetected(qrCodeContent);

			IsStarted = false;

			return null;
		}

#if UNITY_EDITOR || WINDOWS_UWP
		private async Task<Exception> StartAsync(Action qrCodeDetectionStarted)
		{
			Stop();

			if (IsStarted == false)
			{
				try
				{
					if (QRCodeWatcher.IsSupported() == false)
					{
						Log.WriteLine("QRCodeWatcher not suported!");
						return new NotSupportedException("QRCodeWatcher not suported!");
					}

					Log.WriteLine("Check RequestAccessAsync");

					var status = await RequestAccessAsync();

					Log.WriteLine("Check RequestAccessAsync status=" + status);

					// permet d'attendre que RequestAccessAsync soit terminé

					if (status == QRCodeWatcherAccessStatus.Allowed)
					{
						Log.WriteLine("QRCodeWatcher ready!");

						// une fois normalement
						if (_qrCodeWatcher == null)
						{
							_qrCodeWatcher = new QRCodeWatcher();

							_qrCodeWatcher.Updated += OnQRCodeWatcherUpdated;
						}

						//this.QRCode = null;

						_qrCodeWatcher.Start();
						Log.WriteLine("QRCodeWatcher started!");

						IsStarted = true;

						qrCodeDetectionStarted?.Invoke();

						return null;
					}
					else
					{
						Log.WriteLine("RequestAccessAsync for WebCam failed! Status=" + status);
						return new RequestAccessException("The WebCam access is requested!");
					}
				}
				catch (Exception ex)
				{
					Log.WriteLine(ex);
					return ex;
				}
			}

			return null;
		}
#endif

		public string EmulationQrCodeData { get; set; } = "EL4894101";

#if UNITY_EDITOR || WINDOWS_UWP
		private async Task<QRCodeWatcherAccessStatus> RequestAccessAsync(int retry = 0)
		{
			if (retry > 12)
			{
				Log.WriteLine("Too many retry in RequestAccessAsync");
				return QRCodeWatcherAccessStatus.DeniedBySystem;
			}

			var taskRequestAccess = QRCodeWatcher.RequestAccessAsync();

			if (await Task.WhenAny(taskRequestAccess, Task.Delay(5000)) == taskRequestAccess)
			{
				return taskRequestAccess.Result;
			}
			else
			{
				retry++;
				return await RequestAccessAsync(retry);
			}
		}


		private void Stop()
		{
			if (_qrCodeWatcher == null)
			{
				return;
			}

			if (IsStarted == true)
			{
				_qrCodeWatcher.Stop();
				// force la recreation

				_qrCodeWatcher.Updated -= OnQRCodeWatcherUpdated;

				_qrCodeWatcher = null;

				IsStarted = false;
			}
		}
#endif

		// Un QRCode est trouvé même si le dernier est le même !
		public event EventHandler QrCodeFound;

		public event EventHandler QrCodeAdded;

		public event EventHandler QrCodeUpdated;

		public event EventHandler QrCodeRemoved;

		public DetectOnceCancellationSource CreateCancellationSource()
		{
			return new DetectOnceCancellationSource(this);
		}

		// class d'annulation pour DetectOnce
		public class DetectOnceCancellationSource : ICancellationSource
		{
			public DetectOnceCancellationSource(HololensMrtkQrCodeManager qrCodeManager)
			{
				QrCodeManager = qrCodeManager;
			}

			public HololensMrtkQrCodeManager QrCodeManager { get; private set; }

			public bool IsCanceled { get; private set; }

			public bool Cancel()
			{
				if (IsCanceled == true)
				{
					return false;
				}

				if (QrCodeManager != null && QrCodeManager.IsStarted == true)
				{
					// appel le handler de detectOnce qui va appeler le callback completed de la méthode DetectOnce ( et se desabonner du QRCodeChanged)
					QrCodeManager._detectOnceStopCallback?.Invoke(null);

					IsCanceled = true;

					return true;
				}

				return false;
			}
		}
	}

	public enum DetectionQrCodeTypes
	{
		DataAndSpatialLocation,
		DataOnly
	}

	public class QrCodeEventArgs : EventArgs
	{
		public QrCodeEventArgs(QrCodeContent qrCodeContent)
		{
			QrCode = qrCodeContent;
		}

		public QrCodeContent QrCode { get; set; }
	}

	/// <summary>
	/// Exception en cas de RequestAccess no setté
	/// </summary>
	public class RequestAccessException : Exception
	{
		public RequestAccessException(string message) : base(message)
		{
		}
	}
}