using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Astrolabe.Scripts.Tools
{
	public static class FilePicker
	{
		public class FilePickerResult
		{
			public Exception Error { get; set; }

			public string Path { get; set; }

			public bool HaveError => Error != null;

			public bool HavePath => IsCanceled == false && HaveError == false && string.IsNullOrEmpty(Path) == false;

			public bool IsCanceled { get; set; }

			public string SystemAccessToken { get; set; }
		}

		/// <summary>
		/// Ouverture d'un fichier
		/// </summary>
		/// <param name="filters">*.txt</param>
		public static void ChooseFile(Action<FilePickerResult> fileSelected,
			WellKnownLocation location = WellKnownLocation.Unspecified, PickerView pickerView = PickerView.Thumbnail,
			params string[] filters)
		{
			var result = new FilePickerResult();

#if !UNITY_EDITOR && UNITY_WSA_10_0
        UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
        {
            try
            {
                var filepicker = new FileOpenPicker();

                filepicker.ViewMode = (PickerViewMode)(int)pickerView;
                filepicker.SuggestedStartLocation = (PickerLocationId)(int)location;

                if(filters != null && filters.Length > 0)
                {
                    for(int x = 0; x<filters.Length; x++)
                    {
                        var filter = filters[x];

                        if (filter != null)
                        {
                            filepicker.FileTypeFilter.Add(filter);
                        }
                    }
                }
                else
                {
                    filepicker.FileTypeFilter.Add("*");
                }

                StorageFile selectedFile = await filepicker.PickSingleFileAsync();

                if(selectedFile == null)
                {
                    result.IsCanceled = true;
                }
                else if(string.IsNullOrEmpty(selectedFile.Path) == false )
                {
                    result.Path = selectedFile.Path;

			        List<string> invalidTokens = new List<string>();

			        bool isTokenInvalid;

			        foreach (AccessListEntry entry in StorageApplicationPermissions.FutureAccessList.Entries)
			        {
				        isTokenInvalid = false;

				        try
				        {
					        StorageFile file =
 await StorageApplicationPermissions.FutureAccessList.GetFileAsync(entry.Token);

					        // FAL can track files across renames but access rights are not transfered when using standard .net methods like Exists.
					        // Remove file from the token list when it is not accessible anymore. Exists returns false when a file exists but is not accessible due to access rights.
					        isTokenInvalid = !File.Exists(file.Path);
				        }
				        catch (Exception)
				        {
					        isTokenInvalid = true;
				        }
				        if (isTokenInvalid)
				        {
					        invalidTokens.Add(entry.Token);
				        }
			        }

			        foreach (string token in invalidTokens)
			        {
				        StorageApplicationPermissions.FutureAccessList.Remove(token);
			        }

			        StorageApplicationPermissions.FutureAccessList.Add(selectedFile);
                }
                else
                {
                    result.Error = new FileNotFoundException("the path of the file is NULL!");
                }
            }
            catch(Exception ex)
            {
                Log.WriteLine(ex);
                result.Error = ex;
            }

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                fileSelected?.Invoke(result);
            },
            false);
        },
        false);

#elif UNITY_ANDROID && !UNITY_EDITOR
		if (filters != null && filters.Length > 0)
		{
			FileBrowser.SetFilters(true, filters);
		}

		bool success = FileBrowser.ShowLoadDialog(paths =>
		{
			string path = paths[0];
			if (!path.StartsWith("jar:"))
			{
				path = path.Insert(0, "file://");
			}
			result.Path = path;
			if (string.IsNullOrEmpty(result.Path))
			{
				string message = "file path is NULL";
				result.IsCanceled = true;
			}
			else
			{
				Log.WriteLine("File path=" + result.Path, LogMessageType.Information);
			}

			fileSelected?.Invoke(result);
		}, () =>
		{
			string message = "Operation canceled";
			Log.WriteLine(message, LogMessageType.Information);
			result.IsCanceled = true;
			fileSelected?.Invoke(result);
		}, FileBrowser.PickMode.Files, false, null, null, "Load Files", "Load");

#elif UNITY_EDITOR
			result.Path = EditorUtility.OpenFilePanel("Open file", "", "");
			fileSelected?.Invoke(result);
#endif
		}
	}

	public enum PickerView
	{
		//
		// Résumé :
		//     Liste d'éléments.
		List = 0,

		//
		// Résumé :
		//     Un ensemble d'images miniatures.
		Thumbnail = 1
	}

	public enum WellKnownLocation
	{
		//
		// Résumé :
		//     Bibliothèque **Documents**.
		Document = 0,

		//
		// Résumé :
		//     Dossier **Ordinateur**.
		ComputerFolder = 1,

		//
		// Résumé :
		//     Bureau Windows.
		Desktop = 2,

		//
		// Résumé :
		//     Dossier **Téléchargements**.
		Download = 3,

		//
		// Résumé :
		//     Groupe résidentiel.
		HomeGroup = 4,

		//
		// Résumé :
		//     Bibliothèque **Musique**.
		Music = 5,

		//
		// Résumé :
		//     Bibliothèque **Images**.
		Picture = 6,

		//
		// Résumé :
		//     Bibliothèque **Vidéos**.
		Video = 7,

		//
		// Résumé :
		//     Bibliothèque **Objets**.
		Object3D = 8,

		//
		// Résumé :
		//     Emplacement non spécifié.
		Unspecified = 9
	}
}