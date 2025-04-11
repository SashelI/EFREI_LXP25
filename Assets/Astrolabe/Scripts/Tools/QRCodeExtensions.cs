using System;
using Astrolabe.Twinkle;
using Astrolabe.Twinkle.Tools;
#if UNITY_EDITOR || WINDOWS_UWP
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.QR;
#endif
using UnityEngine;

namespace Assets.Astrolabe.Scripts.Tools
{

#if WINDOWS_UWP
using Windows.Perception.Spatial;
using UnityEngine.XR.WSA;
#endif

	public static class QrCodeExtensions
	{
#if UNITY_EDITOR || WINDOWS_UWP
		public static ThreadSafeQrCode ToThreadSafeQrCode(this QRCode qrcode)
		{
			return new ThreadSafeQrCode(qrcode);
		}


		public static QrCodeContentAbsolute ToQrCodeContent(this ThreadSafeQrCode qrcode)
		{
			var code = new QrCodeContentAbsolute();

			code.Data = qrcode.Data;
			code.Id = qrcode.Id;

			var pose = GetSpatialPose(qrcode.SpatialGraphNodeId, qrcode.PhysicalSideLength);

			if (pose != null)
			{
				//Log.WriteLine("qrcode position=" + pose.Value.position.ToString("F6") + " qrcode rotation=" + pose.Value.rotation.ToString("F6"));

				code.SpatialSizeAbsolute = qrcode.PhysicalSideLength;
				code.Pose = pose.Value;

				var p = pose.Value.position;
				var a = pose.Value.rotation.eulerAngles;

				code.SpatialSize = qrcode.PhysicalSideLength.ToLogical();
				code.SpatialPosition = new global::Astrolabe.Twinkle.Vector3(p.x, -p.y, p.z).ToLogical();
				code.SpatialRotation = a.ToVector3(); //new Astrolabe.Twinkle.Vector3(a.x, a.y,-a.z);
				code.HaveSpatialLocation = true;
			}

			return code;
		}
#endif

#if !UNITY_2020_1_OR_NEWER
    public static Pose? GetSpatialPose(Guid spatialGraphNodeId, float physicalSideLength)
    {
        System.Numerics.Matrix4x4? relativePose = System.Numerics.Matrix4x4.Identity;

#if WINDOWS_UWP
                var coordinateSystem =
                  Windows.Perception.Spatial.Preview.SpatialGraphInteropPreview.
                    CreateCoordinateSystemForNode(spatialGraphNodeId);

                if (coordinateSystem == null)
                {
                    return null;
                }

                var rootSpatialCoordinateSystem =
                  (SpatialCoordinateSystem)System.Runtime.InteropServices.Marshal.
                      GetObjectForIUnknown(WorldManager.GetNativeISpatialCoordinateSystemPtr());

                // Get the relative transform from the unity origin
                relativePose = coordinateSystem.TryGetTransformTo(rootSpatialCoordinateSystem);
#endif

        if (relativePose == null)
        {
            return null;
        }

        System.Numerics.Matrix4x4 newMatrix = relativePose.Value;

        // Platform coordinates are all right handed and unity uses left handed matrices.
        // so we convert the matrix
        // from rhs-rhs to lhs-lhs
        // Convert from right to left coordinate system
        newMatrix.M13 = -newMatrix.M13;
        newMatrix.M23 = -newMatrix.M23;
        newMatrix.M43 = -newMatrix.M43;

        newMatrix.M31 = -newMatrix.M31;
        newMatrix.M32 = -newMatrix.M32;
        newMatrix.M34 = -newMatrix.M34;

        System.Numerics.Vector3 scale;
        System.Numerics.Quaternion rotation1;
        System.Numerics.Vector3 translation1;

        System.Numerics.Matrix4x4.Decompose(newMatrix, out scale, out rotation1,
                                            out translation1);

        var translation = new UnityEngine.Vector3(translation1.X, translation1.Y, translation1.Z);
        var rotation = new Quaternion(rotation1.X, rotation1.Y, rotation1.Z, rotation1.W);
        var pose = new Pose(translation, rotation);

        // If there is a parent to the camera that means we are using teleport and we
        // should not apply the teleport to these objects so apply the inverse
        if (CameraCache.Main.transform.parent != null)
        {
            pose = pose.GetTransformedBy(CameraCache.Main.transform.parent);
        }

        //return MovePoseToCenter(pose, qrcode.PhysicalSideLength);
        return pose;
    }
#else

		public static Pose? GetSpatialPose(Guid spatialGraphNodeId, float physicalSideLength)
		{
			var node = spatialGraphNodeId != Guid.Empty ? SpatialGraphNode.FromStaticNodeId(spatialGraphNodeId) : null;

			if (node != null && node.TryLocate(FrameTime.OnUpdate, out var pose))
			{
				if (Camera.main.transform.parent != null)
				{
					pose = pose.GetTransformedBy(Camera.main.transform.parent);
				}

				return pose;
			}

			return null;
		}

#endif


		public static void SetCenteredPose(this QrCodeContent qrCode, LogicalWindow logicalWindow, bool setSize = true)
		{
			var qrCodeAbsolute = qrCode as QrCodeContentAbsolute;

			// On dimensionne + place dans l'espace
			if (setSize)
			{
				logicalWindow.Width = qrCode.SpatialSize;
				logicalWindow.Height = qrCode.SpatialSize;
			}

			if (qrCodeAbsolute != null)
			{
				var pose = qrCodeAbsolute.GetCenteredPosition(logicalWindow.Forward.ToVisual());
				var gameObject = logicalWindow.GetGameObject();

				gameObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
			}
			else
			{
				// ici c'est pour l'éditeur normalement
				logicalWindow.Position.Coordinate = qrCode.SpatialPosition;
				logicalWindow.Angle = qrCode.SpatialRotation;
			}
		}
	}

	public class ThreadSafeQrCode
	{
		public string Data { get; private set; }

		public Guid Id { get; private set; }

		public Guid SpatialGraphNodeId { get; private set; }

		public float PhysicalSideLength { get; private set; }

#if UNITY_2020_1_OR_NEWER && (UNITY_EDITOR || WINDOWS_UWP)

		public ThreadSafeQrCode(QRCode qrCode)
		{
			Data = qrCode.Data;
			Id = qrCode.Id;
			SpatialGraphNodeId = qrCode.SpatialGraphNodeId;
			PhysicalSideLength = qrCode.PhysicalSideLength;
		}

#endif
	}

	public class QrCodeContentAbsolute : QrCodeContent
	{
		public float SpatialSizeAbsolute { get; set; }

		public Pose Pose { get; set; }

		public Pose GetCenteredPosition(float visualWidth, float visualHeight, float visualForward)
		{
			var pose = Pose;

			pose.rotation *= Quaternion.Euler(180, 0, 0);

			// Move the anchor point to the *center* of the QR code
			var deltaToCenterX = visualWidth * 0.5f;
			var deltaToCenterY = visualHeight * 0.5f;

			pose.position += pose.rotation * (deltaToCenterX * UnityEngine.Vector3.right) -
			                 pose.rotation * (visualForward * UnityEngine.Vector3.forward) +
			                 pose.rotation * (deltaToCenterY * UnityEngine.Vector3.down);

			return pose;
		}

		public Pose GetCenteredPosition(float visualForward = 0)
		{
			return GetCenteredPosition(SpatialSizeAbsolute, SpatialSizeAbsolute, visualForward);
		}
	}
}