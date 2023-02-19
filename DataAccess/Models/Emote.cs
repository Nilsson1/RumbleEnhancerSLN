using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class Emote
    {
        public Guid ProfileId { get; set; }
        public string EmoteName { get; set; }
        public byte[] ImageData { get; set; }
    }
}
