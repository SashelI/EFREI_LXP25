using System;
using Astrolabe.Diagnostics;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.IO;
using Astrolabe.Twinkle.Tools;
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{
	[TwinkleLogicalObject("QRCodeMRTK")]
	public class LogicalQrCodeMrtkPosition : LogicalPosition
	{
		public event EventHandler QrCodeStarted;
		public event QrCodeDetectedHandler QrCodeDetected;
		public event QrCodeFailedHandler QrCodeFailed;

		private ICancellationSource _cancellationSource;

		public override void OnAdd(ILogicalWindow parent)
		{
			base.OnAdd(parent);

			if (AutoHide == true)
			{
				if (Window != null)
				{
					Window.Visibility = Visibility.Collapsed;
				}
			}

			Log.WriteLine("Start QRCode MRTK Position");

			_cancellationSource = TwinkleApplication.Instance.Framework.QrCode.DetectOnce(
				() =>
				{
					// Le scan a démarré
					Log.WriteLine("Scan démarré");
					QrCodeStarted?.Invoke(this, EventArgs.Empty);
				},
				(qrCode) =>
				{
					// fin un QRCode est detecté
					//Log.WriteLine("qrcode=" + qrCode.Data + " qrcode.HaveSpatialLocation=" + qrCode.HaveSpatialLocation + " SpatialPosition=" + qrCode.SpatialPosition + " SpatialRotation=" + qrCode.SpatialRotation);

					if (qrCode.HaveSpatialLocation)
					{
						if (qrCode is QrCodeContentAbsolute)
						{
							Log.WriteLine("QrCodeContentAbsolute detected!");

							var qrCodeAbsolute = (QrCodeContentAbsolute)qrCode;

							Pose pose;

							if (ApplyQrCodeSize)
							{
								pose = qrCodeAbsolute.GetCenteredPosition();
							}
							else
							{
								pose = qrCodeAbsolute.GetCenteredPosition(Window.Width.ToVisual(), Window.Height.ToVisual(),
									Window.Forward.ToVisual());
							}

							Window.GetGameObject().transform.SetPositionAndRotation(pose.position, pose.rotation);
						}
						else
						{
							Coordinate = qrCode.SpatialPosition;
							Window.Angle = qrCode.SpatialRotation;
						}

						if (ApplyQrCodeSize)
						{
							Window.Width = qrCode.SpatialSize;
							Window.Height = qrCode.SpatialSize;
						}

						if (AutoHide == true)
						{
							if (Window != null)
							{
								Window.Visibility = Visibility.Visible;
							}
						}
					}

					QrCodeDetected?.Invoke(this, qrCode);
				},
				(exception) =>
				{
					// ici on peut gérer les exceptions
					QrCodeFailed?.Invoke(this, exception);
				}
			);

			//visualRadialView.OnAdd(parent);
		}

		public override void OnRemove()
		{
			base.OnRemove();

			_cancellationSource?.Cancel();
		}

		/// <summary>
		/// Appliquer la taille du QRCode 
		/// </summary>

		[TwinkleLogicalProperty]
		public bool ApplyQrCodeSize { get; set; } = false;

		/// <summary>
		/// Appliquer la taille du QRCode 
		/// </summary>

		[TwinkleLogicalProperty]
		public bool AutoHide { get; set; } = true;
	}

	public delegate void QrCodeDetectedHandler(object sender, QrCodeContent qrCodeContent);

	public delegate void QrCodeFailedHandler(object sender, Exception exception);
}