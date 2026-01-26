using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PathStructure
{
    /// <summary>
    /// Provides shared helper methods for path normalization and logging.
    /// </summary>
    public static class PathStructure_Helpers
    {
        [DllImport("mpr.dll", CharSet = CharSet.Ansi, EntryPoint = "WNetGetConnectionA")]
        private static extern int WNetGetConnection(string lpszLocalName, StringBuilder lpszRemoteName, ref int cbRemoteName);

        /// <summary>
        /// Gets the UNC representation of the provided path.
        /// </summary>
        public static string GetUNCPath(string sFilePath)
        {
            // Check before allocating resources...
            if (sFilePath.StartsWith("\\", StringComparison.Ordinal))
            {
                return sFilePath;
            }

            // Now allocate resources for processing
            var allDrives = DriveInfo.GetDrives();
            var driveType = 0;
            var driveLetter = sFilePath.Substring(0, 3);
            var strBldr = new StringBuilder();

            var uncNameBuilder = new StringBuilder(160);

            for (var i = 0; i < allDrives.Length; i += 1)
            {
                if (allDrives[i].Name == driveLetter)
                {
                    driveType = (int)allDrives[i].DriveType;
                    break;
                }
            }

            if (driveType == 4)
            {
                var uncLength = uncNameBuilder.Capacity;
                var ctr = WNetGetConnection(sFilePath.Substring(0, 2), uncNameBuilder, ref uncLength);

                if (ctr == 0)
                {
                    var uncName = uncNameBuilder.ToString().Trim();
                    for (ctr = 0; ctr < uncName.Length; ctr += 1)
                    {
                        var singleChar = uncName[ctr];
                        var asciiValue = (int)singleChar;
                        if (asciiValue > 0)
                        {
                            strBldr.Append(singleChar);
                        }
                        else
                        {
                            break;
                        }
                    }

                    strBldr.Append(sFilePath.Substring(2));
                    return strBldr.ToString();
                }

                return sFilePath;
            }

            return sFilePath;
        }

        /// <summary>
        /// Occurs when a log message is emitted by the helpers.
        /// </summary>
        public static event EventHandler<string> PathStructureLog;

        /// <summary>
        /// Emits a log message to any registered listeners.
        /// </summary>
        public static void Log(string input)
        {
            PathStructureLog?.Invoke(null, input);
        }
    }
}
