using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Xml.Linq;

namespace RumbleEnhancerWebSite.Models
{
    public class Profile
    {
        //public string ProfileId { get; set; }
        public string ProfileEmail { get; set; }
        public string ProfileName { get; set; }
        public string ProfilePassword { get; set; }
        public string? RumbleURL { get; set; }
        public string? VerificationString { get; set; }
        public bool? VerificationStatus { get; set; }

    }
}
