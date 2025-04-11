using System;
using Assets.Astrolabe.Scripts.HoloLens;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations.Hololens
{
	/// <summary>
	/// Lecteur de QRCode Microsoft
	/// </summary>
	public class HololensMrtkQrCodeOperations : IQrCodeOperations
	{
		private readonly HololensMrtkQrCodeManager _manager = new();

		public HololensMrtkQrCodeOperations()
		{
		}

		public ICancellationSource DetectOnce(Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected,
			Action<Exception> qrCodeException)
		{
			var cancellationSource = new HololensMrtkQrCodeManager.DetectOnceCancellationSource(_manager);

			DetectOnce(cancellationSource, qrCodeDetectionStarted, qrCodeDetected, qrCodeException);

			return cancellationSource;
		}

		private async void DetectOnce(HololensMrtkQrCodeManager.DetectOnceCancellationSource cancellationSource,
			Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
		{
			await _manager.DetectOnceAsync(
				cancellationSource,
				qrCodeDetectionStarted,
				(qrCode) =>
				{
					// contenu du QRCode
					qrCodeDetected(qrCode);
				},
				qrCodeException
			);
		}
	}
}