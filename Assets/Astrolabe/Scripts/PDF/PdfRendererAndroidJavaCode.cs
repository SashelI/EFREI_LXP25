//This is the Java code of Assets/Astrolabe/Libs/pdfrenderer-debug.aar

/*
package com.example.astrolabe.pdfrenderer;

import android.content.Context;
import android.content.res.AssetManager;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.pdf.PdfRenderer;
import android.os.ParcelFileDescriptor;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;

public class PdfRendererHelper
{

	public static PdfRenderer InitDocument(Context context, String fileName, boolean fromStreamingAssets)
	{
		try
		{
			File file;

			if (!fromStreamingAssets)
			{
				file = new File(fileName);
			}
			else
			{
				AssetManager assetManager = context.getAssets();
				// Open the file as an InputStream
				InputStream inputStream = assetManager.open("App/Assets/" + fileName);
				// Write the InputStream content to a temporary file
				file = writeInputStreamToFile(context, inputStream);
			}

			// Create a PdfRenderer
			ParcelFileDescriptor fileDescriptor = ParcelFileDescriptor.open(file, ParcelFileDescriptor.MODE_READ_ONLY);

			// Create a PdfRenderer
			return new PdfRenderer(fileDescriptor);

		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		return null;
	}

	public static byte[] GetDocumentBitmap(PdfRenderer renderer, int pageNumber)
	{
		try
		{
			// Open the specified page
			PdfRenderer.Page page = renderer.openPage(pageNumber);

			// Create a bitmap to render the page
			Bitmap bitmap = Bitmap.createBitmap(page.getWidth(), page.getHeight(), Bitmap.Config.ARGB_8888);

			// Paint bitmap before rendering
			Canvas canvas = new Canvas(bitmap);
			canvas.drawColor(Color.WHITE);
			canvas.drawBitmap(bitmap, 0, 0, null);

			// Render the page onto the bitmap
			page.render(bitmap, null, null, PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);

			// Close the page and the renderer
			page.close();

			byte[] byteArray = bitmapToByteArray(bitmap);

			return byteArray;

		}
		catch (Exception e)
		{
			e.printStackTrace();
		}

		return null;
	}

	public static int[] GetPageSize(PdfRenderer renderer, int pageNumber)
	{
		try
		{
			// Open the specified page
			PdfRenderer.Page page = renderer.openPage(pageNumber);

			int width = page.getWidth();
			int height = page.getHeight();

			// Close the page
			page.close();

			return new int[] { width, height };

		}
		catch (Exception e)
		{
			e.printStackTrace();
		}

		return null;
	}

	public static int GetPageCount(PdfRenderer renderer)
	{
		try
		{
			int pageCount = renderer.getPageCount();

			return pageCount;

		}
		catch (Exception e)
		{
			e.printStackTrace();
			return -1;
		}
	}

	public static void CloseRenderer(PdfRenderer renderer)
	{
		renderer.close();
	}

	private static File writeInputStreamToFile(Context context, InputStream inputStream) throws IOException
	{
		File tempFile = File.createTempFile("temp", null, context.getCacheDir());
        try (FileOutputStream fos = new FileOutputStream(tempFile)) {
            byte[] buffer = new byte[1024];
	int length;
            while ((length = inputStream.read(buffer)) > 0) {
                fos.write(buffer, 0, length);
            }
        }
        return tempFile;
    }

    private static byte[] bitmapToByteArray(Bitmap bitmap)
{
	try
	{
		// Create a ByteArrayOutputStream to hold the byte data
		ByteArrayOutputStream stream = new ByteArrayOutputStream();

		// Compress the bitmap into the stream using PNG format
		bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);

		// Get the byte array from the stream
		byte[] byteArray = stream.toByteArray();

		// Close the stream
		stream.close();

		return byteArray;
	}
	catch (Exception e)
	{
		Log.e("BitmapUtils", "Error converting Bitmap to byte array: " + e.getMessage());
		return null;
	}
}
}
*/

