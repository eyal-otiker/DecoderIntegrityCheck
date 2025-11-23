using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.IO.Compression;

namespace ServerApplicationApi
{
    internal class FileCRUD
    {
        public string NgpFileLocation { get; set; }
        public string DllFileLocation { get; set; }
        public string SettingFileLoction { get; set; }

        private const string LIBRARY_NAME = @"\DecoderLibrary";
        public const string CLIENT_FILES_LOCATION = @"\DecoderLibrary\Files\IcdDataFiles\";

        public void FileCRUDManager(IFormFile decoderFile)
        {
            bool isSavedSuccess = true;
            int indexOfFileType = decoderFile.FileName.LastIndexOf(".ngp");
            string fileZipName = decoderFile.FileName.Remove(indexOfFileType); // name of the file without his type
            string rootFolderSavedFiles = FindAddressOfMainFolder() + CLIENT_FILES_LOCATION;
            string locationZipFileSaved = rootFolderSavedFiles + fileZipName + ".zip";
            string loctionFolderSave = rootFolderSavedFiles + fileZipName + DateTime.Now.ToString("dd-MM-yyyy");
            string locationNgpFileSaved = rootFolderSavedFiles + decoderFile.FileName.Remove(indexOfFileType) + DateTime.Now.ToString("dd-MM-yyyy") + ".ngp";

            if (!Directory.Exists(loctionFolderSave))
            {
                SaveFile(decoderFile, locationZipFileSaved);
                SaveFile(decoderFile, locationNgpFileSaved);

                try
                {
                    ZipFile.ExtractToDirectory(locationZipFileSaved, loctionFolderSave);
                }
                catch (InvalidDataException)
                {
                    isSavedSuccess = false;
                    DeleteFile(locationNgpFileSaved);
                }
                finally
                {
                    DeleteFile(locationZipFileSaved);
                }
            }

            if (isSavedSuccess)
                SetFileLoctions(loctionFolderSave, fileZipName, locationNgpFileSaved);
        }

        private void SaveFile(IFormFile decoderFile, string locationFileSaved)
        {
            using Stream fileStream = new FileStream(locationFileSaved, FileMode.Create);
            decoderFile.CopyTo(fileStream);
            fileStream.Close();
        }

        public static string FindAddressOfMainFolder()
        {
            string currentFolder = Directory.GetCurrentDirectory();
            string parentFolder; int indexOfParentFolder;

            while (!currentFolder.EndsWith(LIBRARY_NAME))
            {
                parentFolder = Directory.GetParent(currentFolder).Name;
                indexOfParentFolder = currentFolder.IndexOf(parentFolder);
                currentFolder = currentFolder.Remove(indexOfParentFolder + parentFolder.Length);
            }

            return currentFolder;
        }

        public static string ReadText(IFormFile icdFile)
        {
            try
            {
                return new StreamReader(icdFile.OpenReadStream()).ReadToEnd();
            }
            catch (ArgumentException)
            {
                //this._logger.Error(ex, "fail to get the content of the ICD file");
                return string.Empty;
            }
        }

        public static string ReadText(string filePath)
        {
            try
            {
                return new StreamReader(filePath).ReadToEnd();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                //this._logger.Error(ex, "fail to get the content of the setting file");
                return string.Empty;
            }
        }

        private void DeleteFile(string dllFileLocation)
        {
            try
            {
                if (dllFileLocation != string.Empty)
                    File.Delete(dllFileLocation);
            }
            catch (System.UnauthorizedAccessException)
            {

            }
        }

        private void SetFileLoctions(string loctionFolderSave, string fileZipName, string locationNgpFileSaved)
        {
            this.NgpFileLocation = locationNgpFileSaved;

            string[] filesLocation = Directory.GetFiles(loctionFolderSave + @"\" + fileZipName);
            for (int i = 0; i < filesLocation.Length; i++) // set location to files in folder
            {
                if (filesLocation[i].EndsWith(".dll"))
                    this.DllFileLocation = filesLocation[i];
                else
                    this.SettingFileLoction = filesLocation[i];
            }

            if (filesLocation.Length != 2)
            {
                DeleteFile(locationNgpFileSaved);
                Directory.Delete(loctionFolderSave, true);
            }
        }

        public static string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
