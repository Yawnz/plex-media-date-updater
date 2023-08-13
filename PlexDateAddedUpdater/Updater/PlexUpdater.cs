using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlexDateAddedUpdater.Helpers;
using PlexDateAddedUpdater.PlexDB;

namespace PlexDateAddedUpdater.Updater
{
    public class PlexUpdater
    {
        public PlexUpdater() 
        {
            this.dbPath = RegistryHelper.GetPlexDataFolderPath();
        }

        public PlexUpdater(string dbPath) 
        {
            this.dbPath = dbPath;
        }

        string dbPath = string.Empty;

        public async Task UpdateAllLibrariesAsyc()
        {
            PlexClient plexClient = new PlexClient(dbPath);

            var libraryFolders = await plexClient.GetLibraryLocationsAsync();

            if (!libraryFolders?.Any() ?? false)
            {
                Console.WriteLine("No library folders detected... Please specify a library folder manually.");
                return;
            }
            int totalFolders = libraryFolders.Count();
            int folderCount = 0;

            Console.WriteLine($"Found {totalFolders} to process.");
            foreach (var libraryFolder in libraryFolders)
            {
                folderCount++;
                Console.Write($"\rProcessing folder {folderCount} of {totalFolders}: {libraryFolder}");
                await UpdateDateAddedFromPathAsync(libraryFolder);
            }
        }

        public async Task UpdateDateAddedFromPathAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Filepath is empty");
                return;
            }
            var files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories);

            if (!files?.Any() ?? false)
            {
                Console.WriteLine($"No files found in {filePath}");
                return;
            }
            int totalFiles = files.Count();

            Console.WriteLine($"Found {totalFiles} files in {filePath}");

            Console.WriteLine("Backing up plex db...");
            string backupFilename = $"{Path.GetFileName(dbPath)}-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            string backupFilePath = Path.Combine(Path.GetDirectoryName(dbPath), backupFilename);

            File.Copy(dbPath, backupFilePath, true);
            Console.WriteLine($"Backed up plex db to {backupFilePath}");

            Console.WriteLine("Trying to connect to PlexDB...");
            PlexClient plexClient = new PlexClient(dbPath);

            await plexClient.CheckTriggers();
            await plexClient.DisableTriggersAsync();
            int fileCount = 0;

            Console.WriteLine("Processing Files...");
            Console.WriteLine("");
            foreach (var file in files)
            {
                fileCount++;
                Console.Write($"\rProcessing File {fileCount} of {totalFiles}: {file}");
                var mediaIds = await plexClient.GetMediaItemIdsAsync(file);

                foreach (var mediaId in mediaIds)
                {
                    if (string.IsNullOrWhiteSpace(mediaId)) continue;
                    var metaDataIds = await plexClient.GetMetaDataItemIdsAsync(mediaId);

                    foreach (var metaDataId in metaDataIds)
                    {
                        if (string.IsNullOrWhiteSpace(metaDataId)) continue;
                        var ogDateAdded = await plexClient.GetDateAddedByMetaDataItemIdAsync(metaDataId);
                        var newDateAdded = DateTimeHelpers.FromEpoch(File.GetCreationTimeUtc(file).ToUnixEpoch());

                        if (!DateTime.Equals(ogDateAdded, newDateAdded))
                        {
                            await plexClient.UpdateDateAddedByMetaDataIdAsync(mediaId, metaDataId, $"{newDateAdded.ToUnixEpoch()}");
                        }
                    }
                }
                Console.WriteLine($"Processed {file}...");
            }
            await plexClient.EnableTriggersAsync();

            Console.WriteLine("Processing complete.");
            Console.WriteLine($"Processed {fileCount} files out of {totalFiles}");
        }
    }
}
