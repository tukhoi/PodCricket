using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace PodCricket.Utilities.Helpers
{
    public class StorageHelper
    {
        public static async Task<Stream> OpenStreamForWriteAsync(string fileName, bool createBackup = true)
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            if (createBackup && CheckLocalFileExist(fileName))
            {
                StorageFile localFile = null;
                localFile = await localFolder.GetFileAsync(fileName);

                if (localFile != null)
                    await localFile.RenameAsync(fileName + ".bak", NameCollisionOption.ReplaceExisting);
            }

            var newLocalFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            return await newLocalFile.OpenStreamForWriteAsync();
        }

        public static async Task<Stream> OpenStreamForReadAsync(string fileName, bool tryBackup = true)
        {
            var finalFile = fileName;

            if (!CheckLocalFileExist(fileName) && tryBackup)
            {
                if (CheckLocalFileExist(fileName + ".bak"))
                    finalFile = fileName + ".bak";
                else
                    return null;
            }
            
            var storageFile = await OpenFileForReadAsync(finalFile);
            return await storageFile.OpenStreamForReadAsync();
        }

        private static async Task<StorageFile> OpenFileForReadAsync(string fileName)
        {
            StorageFile storageFile = null;
            try
            {
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/" + fileName));
            }
            catch (FileNotFoundException)
            {
                storageFile = null;
            }

            return storageFile;
        }

        public static bool CheckLocalFileExist(string filePath)
        {
            return IsolatedStorageFile.GetUserStoreForApplication().FileExists(filePath);
        }

        public static Stream GetFileStream(string filePath)
        {
            if (IsolatedStorageFile.GetUserStoreForApplication().FileExists(filePath))
                return IsolatedStorageFile.GetUserStoreForApplication().OpenFile(filePath, FileMode.Open, FileAccess.Read);

            return null;
        }

        /// <summary>
        /// For now just igonre .bak file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="alsoBackup"></param>
        /// <returns></returns>
        public static async Task DeleteAsync(string fileName, bool alsoBackup = true)
        {
            StorageFile storageFile = null;
            try
            {
                storageFile = await Windows.Storage.StorageFile
                    .GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/" + fileName));
                
                await storageFile.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                return;
            }
        }

        public static bool DeleteFile(string fileName)
        {
            if (IsolatedStorageFile.GetUserStoreForApplication().FileExists(fileName))
            {
                IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(fileName);
                return true;
            }

            return false;
        }

        public static bool CopyFile(string sourceFile, string destFile)
        {
            try
            {

                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(destFile))
                        isoStore.DeleteFile(destFile);

                    isoStore.CopyFile(sourceFile, destFile, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool DeleteDownloadedStream(string fileName)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(fileName))
                        isoStore.DeleteFile(fileName);

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> MoveFile(string sourceFile, string destinationFile)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(destinationFile))
                        isoStore.DeleteFile(destinationFile);

                    isoStore.MoveFile(sourceFile, destinationFile);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void SaveConfig(string key, string value)
        {
            IsolatedStorageSettings isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            isolatedStore.Remove(key);
            isolatedStore.Add(key, value);
            isolatedStore.Save();
        }

        public static bool LoadConfig(string key, out string result)
        {
            IsolatedStorageSettings isolatedStore = IsolatedStorageSettings.ApplicationSettings;

            result = "";
            try
            {
                result = (string)isolatedStore[key];
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
