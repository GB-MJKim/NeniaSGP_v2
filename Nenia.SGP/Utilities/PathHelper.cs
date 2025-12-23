using System;
using System.IO;

namespace Nenia.SGP.Utilities
{
    public static class PathHelper
    {
        private const string AppFolderName = "NeniaSGP_v2";

        public static string GetAppDataRoot()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppFolderName);

            Directory.CreateDirectory(root);
            return root;
        }

        public static string GetDbPath()
        {
            var data = Path.Combine(GetAppDataRoot(), "Data");
            Directory.CreateDirectory(data);
            return Path.Combine(data, "products.db");
        }
    }
}
