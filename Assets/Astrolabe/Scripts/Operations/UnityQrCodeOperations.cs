using UnityEngine;
using Astrolabe.Twinkle;
using System;

/*
public class UnityQRCodeOperations : IQRCodeOperations
{
    private Transform webCamComponentTransform;

    public UnityQRCodeOperations(Transform webCamComponentTransform)
    {
        this.webCamComponentTransform = webCamComponentTransform;
    }

    public ICancellationSource DetectOnce(Action<string> qrCodeDetected)
    {
        return new QRCodeManager(this.webCamComponentTransform).DetectOnce(
            (e) =>
            {
                // contenu du QRCode
                qrCodeDetected(e.Text);
            });
    }
}
*/