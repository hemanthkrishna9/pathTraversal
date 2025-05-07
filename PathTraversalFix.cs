using System;
using System.IO;

namespace SecurityUtils
{
    public class PathValidator
    {
        public static bool IsPathSafe(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return false;
            
            try
            {
                string decodedPath = Uri.UnescapeDataString(inputPath);
                
                string[] parts = decodedPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, 
                                            StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    if (part == "..")
                        return false;
                }
                
                Path.GetFullPath(decodedPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
