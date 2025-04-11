#if WINDOWS_UWP
using UnityEngine;
using Astrolabe.Twinkle;
using System;

/// <summary>
/// Lecteur de QRCode Microsoft
/// </summary>

public class HololensZXingQRCodeOperations : IQrCodeOperations
{
    HololensZXingQRCodeManager manager = new HololensZXingQRCodeManager();

    public HololensZXingQRCodeOperations()
    {
    }

    public ICancellationSource DetectOnce(Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
    {
        var cancellationSource = new HololensZXingQRCodeManager.DetectOnceCancellationSource(manager);

        DetectOnce(cancellationSource, qrCodeDetectionStarted, qrCodeDetected, qrCodeException);

        return cancellationSource;
    }

    private async void DetectOnce(HololensZXingQRCodeManager.DetectOnceCancellationSource cancellationSource, Action qrCodeDetectionStarted, Action<QrCodeContent> qrCodeDetected, Action<Exception> qrCodeException)
    {
        await manager.DetectOnceAsync(
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
#endif