using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitTorrentMusic
{
    public class Song : ISong
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public int Year { get; set; }
        public TimeSpan Duration { get; set; }
        public int Size { get; set; }  // in bytes - agree with FileTransfer
        public string[] Featuring { get; set; } = Array.Empty<string>();
        public string Hash { get; set; } = "";
    }

}
