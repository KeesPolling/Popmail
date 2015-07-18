using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PopMailDemo.Tests.EmailProxies
{
    public class PrepareMockData
    {
        public async Task<List<StorageFile>> UnZip(string ZipName)
        {
            var files = new List<StorageFile>();

            // open zip
            var destination = ApplicationData.Current.LocalFolder;
            var deployedZip = await Package.Current.InstalledLocation.GetFileAsync(ZipName);
            var zipFile = await deployedZip.CopyAsync(ApplicationData.Current.LocalFolder);
            using (var zipStream = await zipFile.OpenStreamForReadAsync())
            {
                using (var archive = new ZipArchive(zipStream))
                {
                    var entries = archive.Entries;
                    // iterate through zipped objects
                    foreach (var archiveEntry in entries)
                    {
                        await UnzipZipArchiveEntryAsync(archiveEntry, archiveEntry.FullName, destination);
                    }
                }
            }
            return files;
        }
        /// <summary>
        /// It checks if the specified path contains directory.
        /// </summary>
        /// <param name="entryPath">The specified path</param>
        /// <returns></returns>
        private static bool IfPathContainDirectory(string entryPath)
        {
            if (string.IsNullOrEmpty(entryPath))
            {
                return false;
            }

            return entryPath.Contains("/");
        }

        /// <summary>
        /// It checks if the specified folder exists.
        /// </summary>
        /// <param name="storageFolder">The container folder</param>
        /// <param name="subFolderName">The sub folder name</param>
        /// <returns></returns>
        private static async Task<bool> IfFolderExistsAsync(StorageFolder storageFolder, string subFolderName)
        {
            try
            {
                IStorageItem item = await storageFolder.GetItemAsync(subFolderName);
                return (item != null);
            }
            catch
            {
                // Should never get here
                return false;
            }
        }
        /// <summary>
        /// Unzips ZipArchiveEntry asynchronously.
        /// </summary>
        /// <param name="entry">The entry which needs to be unzipped</param>
        /// <param name="filePath">The entry's full name</param>
        /// <param name="unzipFolder">The unzip folder</param>
        /// <returns></returns>
        private static async Task UnzipZipArchiveEntryAsync(ZipArchiveEntry entry, string filePath, StorageFolder unzipFolder)
        {
            if (IfPathContainDirectory(filePath))
            {
                // Create sub folder
                string subFolderName = Path.GetDirectoryName(filePath);

                bool isSubFolderExist = await IfFolderExistsAsync(unzipFolder, subFolderName);

                StorageFolder subFolder;

                if (!isSubFolderExist)
                {
                    // Create the sub folder.
                    subFolder =
                        await unzipFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.ReplaceExisting);
                }
                else
                {
                    // Just get the folder.
                    subFolder =
                        await unzipFolder.GetFolderAsync(subFolderName);
                }

                // All sub folders have been created yet. Just pass the file name to the Unzip function.
                string newFilePath = Path.GetFileName(filePath);

                if (!string.IsNullOrEmpty(newFilePath))
                {
                    // Unzip file iteratively.
                    await UnzipZipArchiveEntryAsync(entry, newFilePath, subFolder);
                }
            }
            else
            {

                // Read uncompressed contents
                using (Stream entryStream = entry.Open())
                {
                    byte[] buffer = new byte[entry.Length];
                    entryStream.Read(buffer, 0, buffer.Length);

                    // Create a file to store the contents
                    StorageFile uncompressedFile = await unzipFolder.CreateFileAsync
                    (entry.Name, CreationCollisionOption.ReplaceExisting);

                    // Store the contents
                    using (IRandomAccessStream uncompressedFileStream =
                    await uncompressedFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream outstream = uncompressedFileStream.AsStreamForWrite())
                        {
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }
            }
        }

    }

}
