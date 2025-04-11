using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;

namespace Assets.Astrolabe.Scripts.PDF
{
	public class AndroidPdfGenerator : BasePdfGenerator
	{
		private const string ANDROID_STREAMINGASSETS_PATH_BEGIN = "jar:file:///";
		private const string ANDROID_STREAMINGASSETS_PATH_END = "base.apk!/assets";

		public AndroidPdfGenerator()
		{
		}

		public new async Task LoadAsync(string fileName)
		{
			Close();

			pdfFilename = fileName;

			IsLoaded = false;
			IsLoading = true;

			try
			{
				var fromStreamingAssets = fileName.Contains(ANDROID_STREAMINGASSETS_PATH_BEGIN) &&
				                          fileName.Contains(ANDROID_STREAMINGASSETS_PATH_END);
				pdfInstance.OpenDocument(fileName, fromStreamingAssets);

				//On ne charge que la première page pour affichage, puis on lance le loading des autres pages en background task
				pageCount = await pdfInstance.GetPageCountAsync(fileName, PdfInstance.PageLoadType.First);

				pdfInstance.StartPageLoadingThread(0);
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.Message, LogMessageType.Error);
			}
			finally
			{
				IsLoading = false;
				IsLoaded = true;
			}

			PageIndex = 0;
		}
	}
}