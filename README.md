# Plex Media "Date Added" Updater

This is a simple CLI app written in C# to automate updating the "date added" date for media items in your Plex Media Server Library. This affects the order of media items when sorted by "Date Added" or view the "Recently Added" queue.
Not happy with the arrangement of your "Recently Added" media items? Use this tool to correct their dates to those of the file creation dates.

The CLI requires 2 inputs: 
- The path to your Plex Library DB (will default to your default Plex data directory if left blank).
- The path to the directory where your media items are located (will scan all your library folders if left blank).

**Please scan your library files again after running the tool to ensure the changes reflect correctly**

You can download the latest release if you simply want to use the CLI.
You can download and compile the source code if you want to make the CLI suit your needs.

Requires C#10 support to compile.
