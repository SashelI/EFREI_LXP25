using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Diagnostics;

namespace Assets.Astrolabe.Scripts.PDF
{
	public class BasePdfGenerator
	{
		protected int pageCount;
		protected string pdfFilename = string.Empty;

		protected PdfInstance pdfInstance;

		public Guid Id => _id;

		private readonly Guid _id = Guid.NewGuid();

		public BasePdfGenerator()
		{
			pdfInstance = new PdfInstance();
		}

		public bool IsLoading { get; protected set; } = false;

		public bool IsLoaded { get; protected set; } = false;

		public void Close()
		{
			pdfInstance.CloseDocument();

			IsLoading = false;
			IsLoaded = false;
		}

		public async Task LoadAsync(string fileName)
		{
			Close();

			IsLoaded = false;
			IsLoading = true;

			try
			{
				pdfInstance.OpenDocument(fileName);

				pdfFilename = fileName;

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

		public int PageCount => pageCount;

		public int PageIndex
		{
			get => _pageIndex;

			set
			{
				if (pageCount == 0)
				{
					return;
				}

				if (IsLoading)
				{
					return;
				}

				if (value >= 0 && value < PageCount)
				{
					_pageIndex = value;

					PageIndexChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private int _pageIndex = -1;

		public event EventHandler PageIndexChanged;

		public async Task<RasterizedPage> GetPageAsync()
		{
			try
			{
				if (PageIndex == -1)
				{
					return null;
				}

				if (pageCount == 0)
				{
					return null;
				}

				var pageExport = new RasterizedPage();

				var skbmp = await pdfInstance.GetPageBitmap(pdfFilename, _pageIndex);
				pdfInstance.StartPageLoadingThread(_pageIndex);

				pageExport.Pixels = skbmp.Bytes;

				pageExport.Width = skbmp.Width;
				pageExport.Height = skbmp.Height;

				return pageExport;
			}
			catch (Exception ex)
			{
				Log.WriteLine("Id=" + Id + " Exception=" + ex, LogMessageType.Error);
				return null;
			}
		}
	}
}