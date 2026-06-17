using System;
using System.Collections.Generic;
using System.Text;

namespace FileZen.Models
{
    class CleanupResult
    {

        public int FilesDeleted { get; set; }
        public long SpaceFreedMB { get; set; }
        public string Message { get; set; }
    }
}
