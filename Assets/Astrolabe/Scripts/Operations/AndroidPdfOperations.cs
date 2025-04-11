using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.PDF;
using Astrolabe.Core.Framework.Pdf;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations
{
	public class AndroidPdfOperations : IPdfOperations
	{
		public IPdfOperationInstance CreateInstance()
		{
			return new AndroidPdfOperationInstance();
		}
	}

	public class AndroidPdfOperationInstance : IPdfOperationInstance
	{
		private readonly AndroidPdfGenerator _generator = new();

		public bool IsLoaded => _generator.IsLoaded;

		public bool IsLoading => _generator.IsLoading;

		public int PageCount => _generator.PageCount;

		public int PageIndex
		{
			get => _generator.PageIndex;

			set => _generator.PageIndex = value;
		}

		public event EventHandler PageIndexChanged
		{
			add => _generator.PageIndexChanged += value;

			remove => _generator.PageIndexChanged -= value;
		}

		public void Close()
		{
			_generator.Close();
		}

		public async Task<IRasterizedPdfPage> GetPageAsync()
		{
			var rasterizedPage = await _generator.GetPageAsync();
			return new AndroidRasterizedPdfPage(rasterizedPage);
		}

		public async Task LoadAsync(string pdfFilename)
		{
			await _generator.LoadAsync(pdfFilename);
		}

		public PixelFormat GetPixelFormat()
		{
			//return PixelFormat.RGBA32; Si utilisation de android natif
			return PixelFormat.BGRA32;
		}
	}

	public class AndroidRasterizedPdfPage : IRasterizedPdfPage
	{
		public AndroidRasterizedPdfPage(RasterizedPage page)
		{
			Width = page.Width;
			Height = page.Height;
			Pixels = page.Pixels;
		}

		public int Width { get; set; }
		public int Height { get; set; }
		public byte[] Pixels { get; set; }
	}
}