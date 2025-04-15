using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Astrolabe.Diagnostics;
using PDFtoImage;
using SkiaSharp;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace Assets.Astrolabe.Scripts.Tools
{
	public class PdfInstance
	{
		public int MaxLongSideSize { get; set; } = 1280;

		private readonly object _isLoadingLock = new();
		private readonly object _pagesCacheLock = new();
		private readonly object _pageCountLock = new();

#if UNITY_ANDROID && !UNITY_EDITOR
		private MemoryStream _documentStream;
#else
		private FileStream _documentStream;
#endif

		private SKBitmap[] _documentPagesCache;

		private bool _isLoading = false;
		private bool _firstPageHasBeenLoaded = false;

		private int _pageCount = -1;

		private Thread _pageLoadingThread;

		public PdfInstance(int pageCount)
		{
			_pageCount = pageCount;
			_documentPagesCache = new SKBitmap[pageCount];
		}

		public PdfInstance()
		{
		}

		/// <summary>
		/// Opens document stream for later acces.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="fromStreamingAssets">(Android) is the file coming from streaming assets folder</param>
		public void OpenDocument(string filePath, bool fromStreamingAssets = false)
		{
			_documentStream?.Close();
			_firstPageHasBeenLoaded = false;

			try
			{
#if UNITY_ANDROID && !UNITY_EDITOR
			FileHelper.GetMemoryStreamFromPathAndroid(filePath, fromStreamingAssets, out _documentStream);
#else
				_documentStream = File.OpenRead(filePath);
#endif
			}
			catch (Exception e)
			{
				Log.WriteLine("Error while opening file " + filePath + " : \r\n" + e.Message, LogMessageType.Error);
			}
		}

		/// <summary>
		/// Gets number of pages in the pdf. If not disabled, loads every page in memory as bitmaps
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="loadType">Wether we start loading all pages, only the first one or no pages.</param>
		/// <returns></returns>
		public async Task<int> GetPageCountAsync(string filePath, PageLoadType loadType = PageLoadType.All)
		{
			if (_documentStream == null)
			{
				OpenDocument(filePath);
			}

			if (_documentStream != null)
			{
				if (_pageCount == -1)
				{
					_pageCount = Conversion.GetPageCount(_documentStream, true);
				}

				if (_documentPagesCache == null)
				{
					_documentPagesCache = new SKBitmap[_pageCount];
				}

				if (loadType == PageLoadType.All)
				{
					StartPageLoadingThread();
				}
				else if (loadType == PageLoadType.First)
				{
					ClearPagesCache();

					_documentPagesCache[0] = Conversion.ToImage(_documentStream, page: 0, options: SetRenderOptions(0),
						leaveOpen: true);
					await Task.Delay(5);

					_firstPageHasBeenLoaded = true;
				}

				return _pageCount;
				//ATTENTION : on laisse le stream ouvert. Penser à refermer le document PDF en appelant CloseDocument.
			}
			else
			{
				Log.WriteLine("Document stream is null.", LogMessageType.Error);
			}

			return -1;
		}

		/// <summary>
		/// Starts page loading thread for all or current page (+ previous and next)
		/// </summary>
		/// <param name="pageIndex">If = -1, loads all pages. Else, loads pages adjacent to this index.</param>
		public void StartPageLoadingThread(int pageIndex = -1)
		{
			var isAlreadyRunning = _pageLoadingThread != null && _pageLoadingThread.IsAlive;

			if (!isAlreadyRunning)
			{
				_pageLoadingThread = new Thread(() => LoadPagesInBackground(pageIndex));
				_pageLoadingThread.Start();
			}
		}

		/// <summary>
		/// Loads pages bmp in memory as a background thread. if not all pages, loads current index + previous and next page.
		/// </summary>
		/// <returns>True if successfull</returns>
		private void LoadPagesInBackground(int pageIndex = -1)
		{
			lock (_isLoadingLock)
			{
				_isLoading = true;
			}

			try
			{
				if (_documentStream != null)
				{
					if (_pageCount == -1)
					{
						lock (_pageCountLock)
						{
							_pageCount = Conversion.GetPageCount(_documentStream, true);
						}
					}

					var startIndex = 0;
					var endIndex = _pageCount;

					SKBitmap prevPage = null;
					SKBitmap curtPage = null;
					SKBitmap nextPage = null;

					if (pageIndex != -1)
					{
						startIndex = Math.Max(0, pageIndex - 1);
						endIndex = Math.Min(pageIndex + 1, _pageCount - 1);

						var hasPrevPage = startIndex != pageIndex;
						var hasNextPage = endIndex != pageIndex;

						KeepOnlyInCache(new List<int>
							{ startIndex, pageIndex, endIndex }); //On clear pour ne garder qu'au max. 3 pages en cache

						lock (_pagesCacheLock)
						{
							if (hasPrevPage)
							{
								if (_documentPagesCache[startIndex] == null)
								{
									_documentPagesCache[startIndex] = Conversion.ToImage(_documentStream,
										page: startIndex, options: SetRenderOptions(startIndex), leaveOpen: true);
									Thread.Sleep(5);
								}
							}

							if (hasNextPage)
							{
								if (_documentPagesCache[endIndex] == null)
								{
									_documentPagesCache[endIndex] = Conversion.ToImage(_documentStream, page: endIndex,
										options: SetRenderOptions(endIndex), leaveOpen: true);
									Thread.Sleep(5);
								}
							}

							if (_documentPagesCache[pageIndex] == null)
							{
								_documentPagesCache[pageIndex] = Conversion.ToImage(_documentStream, page: pageIndex,
									options: SetRenderOptions(pageIndex), leaveOpen: true);
								Thread.Sleep(5);
							}
						}
					}
					else
					{
						ClearPagesCache();

						for (var i = startIndex; i < endIndex; i++)
						{
							if (i == 0 && _firstPageHasBeenLoaded)
							{
								continue;
							}

							lock (_pagesCacheLock)
							{
								_documentPagesCache[i] = Conversion.ToImage(_documentStream, page: i,
									options: SetRenderOptions(i), leaveOpen: true);
								Thread.Sleep(5);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.WriteLine("Document pages loading failed : " + e.Message, LogMessageType.Error);
			}
			finally
			{
				lock (_isLoadingLock)
				{
					_isLoading = false;
				}
			}
		}


		private IEnumerator LoadAllPagesCoroutine()
		{
			_isLoading = true;

			var images = Conversion.ToImages(_documentStream, true);

			var it = images.GetEnumerator();
			for (var i = 0; i < _documentPagesCache.Length; i++)
			{
				_documentPagesCache[i] = it.Current;
				it.MoveNext();
				i++;
			}

			it.Dispose();

			yield return null;

			_isLoading = false;
		}

		/// <summary>
		/// Gets page from document. If page is not already cached, we go and grab it from current document stream.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="pageIndex"></param>
		/// <returns></returns>
		public async Task<SKBitmap> GetPageBitmap(string fileName, int pageIndex)
		{
			//Si la page est en cache, on la récupère
			if (_documentPagesCache.Length > pageIndex)
			{
				var page = _documentPagesCache[pageIndex];

				if (page != null)
				{
					return _documentPagesCache[pageIndex];
				}
			}

			//Si la récupération des pages est en cours sur le background thread, on attend
			if (_isLoading)
			{
				var millisecondsTillTimeout = 2000; //Temps avant d'abandonner

				var page = _documentPagesCache[pageIndex];

				while (millisecondsTillTimeout > 0 && page == null)
				{
					await Task.Delay(5);
					millisecondsTillTimeout -= 5;
				}

				if (millisecondsTillTimeout > 0)
				{
					return page;
				}
			}

			//Si l'on ne charge pas les page ou qu'on a un timeout, on tente la récupération directe
			if (_documentStream == null)
			{
				OpenDocument(fileName);
			}

			if (_documentStream != null)
			{
				return Conversion.ToImage(_documentStream, page: pageIndex, options: SetRenderOptions(pageIndex),
					leaveOpen: true);
			}

			return null;
		}

		private RenderOptions SetRenderOptions(int pageIndex)
		{
			var pageSize = Conversion.GetPageSize(_documentStream, page: pageIndex, leaveOpen: true);
			if (pageSize.Width > pageSize.Height)
			{
				return new RenderOptions(AntiAliasing: PdfAntiAliasing.Text, WithAspectRatio: true,
					Width: MaxLongSideSize);
			}
			else
			{
				return new RenderOptions(AntiAliasing: PdfAntiAliasing.Text, WithAspectRatio: true,
					Height: MaxLongSideSize);
			}
		}

		public void CloseDocument()
		{
			_firstPageHasBeenLoaded = false;

			if (_documentStream != null)
			{
				_documentStream.Close();
				_documentStream = null;

				StopPageLoadingThread();
				ClearPagesCache();

				_pageLoadingThread = null;
				_documentPagesCache = null;

#if UNITY_EDITOR
				UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#else
				Resources.UnloadUnusedAssets();
#endif
			}
		}

		public void StopPageLoadingThread()
		{
			_isLoading = false;

			if (_pageLoadingThread != null && _pageLoadingThread.IsAlive)
			{
				_pageLoadingThread.Abort();
			}
		}

		private void ClearPagesCache()
		{
			for (var i = 0; i < _documentPagesCache.Length; i++)
			{
				_documentPagesCache[i]?.Dispose();
				_documentPagesCache[i] = null;
			}
		}

		/// <summary>
		/// Will clear cache except for indexes in parameter
		/// </summary>
		/// <param name="indexesToKeep"></param>
		private void KeepOnlyInCache(List<int> indexesToKeep = null)
		{
			if (indexesToKeep != null)
			{
				for (var i = 0; i < _documentPagesCache.Length; i++)
				{
					if (!indexesToKeep.Contains(i))
					{
						_documentPagesCache[i]?.Dispose();
						_documentPagesCache[i] = null;
					}
				}
			}
		}

		private void OnApplicationQuit()
		{
			StopPageLoadingThread();
		}

		public enum PageLoadType
		{
			All,
			First,
			None
		}

		~PdfInstance()
		{
			CloseDocument();
		}
	}
}