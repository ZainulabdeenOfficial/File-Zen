using System;
using System.Collections.Generic;
using System.Text;

namespace FileZen.Models
{
   public  class FileItem
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
    }
}
