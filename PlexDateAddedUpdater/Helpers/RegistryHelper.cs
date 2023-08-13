using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlexDateAddedUpdater.Helpers
{
    public static class RegistryHelper
    {
        public static string GetPlexDataFolderPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine($"{Registry.GetValue("HKEY_CURRENT_USER\\Software\\Plex, Inc.\\Plex Media Server",
                    "LocalAppDataPath",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))}",
                    "Plex Media Server\\Plug-in Support\\Databases\\com.plexapp.plugins.library.db");
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Plex Media Server\\Plug-in Support\\Databases\\com.plexapp.plugins.library.db");
            }
        }
    }
}
