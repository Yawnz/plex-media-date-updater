// See https://aka.ms/new-console-template for more information

using PlexDateAddedUpdater.Helpers;
using PlexDateAddedUpdater.Updater;

Console.WriteLine("Welcome to the plex recently added fixer");
Console.WriteLine("");
Console.WriteLine("Please enter the plex DB path:");
string dbPath = Console.ReadLine();

if (ConsoleHelpers.IsExitInput(dbPath)
    || string.IsNullOrWhiteSpace(dbPath)
    || !File.Exists(dbPath))
{
    Console.WriteLine("File does not exist or is inaccessible. Quitting.");
    return;
}

string lastInput = string.Empty;
PlexUpdater plexUpdater = new PlexUpdater(dbPath);

while (!ConsoleHelpers.IsExitInput(lastInput))
{
    Console.Write($"\rPlease enter the path of the folder to check for media items:");
    Console.WriteLine("");
    lastInput = Console.ReadLine();

    if (ConsoleHelpers.IsExitInput(lastInput)) break;
    if (string.IsNullOrWhiteSpace(lastInput) || !Directory.Exists(lastInput)) continue;

    _ = plexUpdater.UpdateDateAddedFromPathAsync(lastInput);
}


