using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Models
{
    public class Profile
    {
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ProfilePassword { get; set; }
        public string ProfileEmail { get; set; }
        public int ProfileVerified { get; set; }
        
    }
}
