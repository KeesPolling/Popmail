using System;

namespace PopMailDemo.Tests
{
    public class PrepareMockData
    {
        private async static Task<List<StorageFile>> UnZip()
        {
            var files = new List();

            // open zip
            var zipFile = await Package.Current.InstalledLocation.GetFileAsync("Data.zip");
            using (var zipStream = await zipFile.OpenReadAsync())
            {
                using (var archive = new ZipArchive(zipStream.AsStream()))
                {
                    // iterate through zipped objects
                    foreach (var archiveEntry in archive.Entries)
                    {
                        using (var outStream = archiveEntry.Open())
                        {
                            // unzip file to app's LocalFolder
                            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(archiveEntry.Name);
                            using (var inStream = await file.OpenStreamForWriteAsync())
                            {
                                await outStream.CopyToAsync(inStream);
                            }
                            files.Add(file);
                        }
                    }
                }
            }
        }
    }
}