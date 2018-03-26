using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ImageFilter
{
    public class TestImages
    {
        private static IEnumerable<FileInfo> images;

        public static IEnumerable<FileInfo> GetTestImagesFromTestFolder(string additionalPath)
        {
            if (images != null) return images;

            var directory =
                new DirectoryInfo(Path.GetFullPath(
                    TestContext.CurrentContext.TestDirectory + "../../../Images" + additionalPath)
                );
            images = GetFilesByExtensions(directory, ".bmp");

            return images;
        }

        private static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo directory,
            params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }

            FileInfo[] files = directory.GetFiles();
            return files.Where(f => extensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
        }
    }
}