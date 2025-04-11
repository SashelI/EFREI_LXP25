using System;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.PDF;
using Astrolabe.Core.Framework.Pdf;
using Astrolabe.Twinkle;

namespace Assets.Astrolabe.Scripts.Operations
{
#if UNITY_EDITOR
	public class EditorPdfOperations : IPdfOperations
	{
		public IPdfOperationInstance CreateInstance()
		{
			return new EditorPdfOperationsInstance();
		}
	}

	public class EditorPdfOperationsInstance : IPdfOperationInstance
	{
		private readonly EditorPdfGenerator _generator = new();

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
			return new EditorRasterizedPdfPage(rasterizedPage);
		}

		public async Task LoadAsync(string pdfFilename)
		{
			await _generator.LoadAsync(pdfFilename);
		}

		public PixelFormat GetPixelFormat()
		{
			return PixelFormat.BGRA32;
		}
	}

	public class EditorRasterizedPdfPage : IRasterizedPdfPage
	{
		public EditorRasterizedPdfPage(RasterizedPage page)
		{
			Width = page.Width;
			Height = page.Height;
			Pixels = page.Pixels;
		}

		public int Width { get; set; }
		public int Height { get; set; }
		public byte[] Pixels { get; set; }
	}
#endif
}