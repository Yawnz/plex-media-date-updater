// See https://aka.ms/new-console-template for more information

using PlexDateAddedUpdater.Helpers;
using PlexDateAddedUpdater.Updater;

var plexDataPath = RegistryHelper.GetPlexDataFolderPath();

Console.WriteLine("Welcome to the plex recently added fixer");
Console.WriteLine("");
Console.WriteLine($"Please enter the plex DB path (default: {plexDataPath}):");
string dbPath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(dbPath)) dbPath = RegistryHelper.GetPlexDataFolderPath();
if (ConsoleHelpers.IsExitInput(dbPath)
    || string.IsNullOrWhiteSpace(dbPath)
    || !File.Exists(dbPath))
{
    Console.WriteLine("File does not exist or is inaccessible. Quitting.");
    return;
}

PlexUpdater plexUpdater = new PlexUpdater(dbPath);
string lastInput = string.Empty;

while (!ConsoleHelpers.IsExitInput(lastInput))
{
    Console.Write($"\rPlease enter the path of the folder to check for media items (default: all libraries will be scanned):");
    Console.WriteLine("");
    lastInput = Console.ReadLine();

    if (ConsoleHelpers.IsExitInput(lastInput)) break;
    if (string.IsNullOrWhiteSpace(lastInput))
    {
        _ = plexUpdater.UpdateAllLibrariesAsyc();
    }
    else if (!Directory.Exists(lastInput))
    {
        Console.WriteLine("Invalid directory. Please try again.");
        continue;
    }
    else
    {
        _ = plexUpdater.UpdateDateAddedFromPathAsync(lastInput);
    }
}


