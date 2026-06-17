using System;
using System.Collections.Generic;
using System.IO;
using FileZen.Models;

namespace FileZen.Core
{
    public class FileScanner
    {
        public  List<FileItem> Scan(string path)
        {
            var list = new List<FileItem>();

            if (!Directory.Exists(path))
                return list;

            foreach (var file in Directory.GetFiles(path))
            {
                var info = new FileInfo(file);

                list.Add(new FileItem
                {
                    FullPath = file,
                    Name = info.Name,
                    Extension = info.Extension,
                    Size = info.Length,
                    Created = info.CreationTime
                });
            }

            return list;
        }
    }
}