using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RumbleEnhancerWebSite.Models;
using System.Data;
using System.Drawing;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

namespace RumbleEnhancerWebSite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            DashboardModel dashboard = new DashboardModel();
            dashboard.Emotes = DBManager.GetProfileEmotes(DBManager.GetProfileId(User.Identity.Name));

            foreach (var emote in dashboard.Emotes)
            {
                using(var ms = new MemoryStream(emote.ImageData))
                {
                    Image emoteFile = Image.FromStream(ms);
                }
            }
            return View(dashboard);
        }

        public IActionResult UploadEmote()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadEmote(EmoteModel emote)
        {
            string filename = Path.GetFileNameWithoutExtension(emote.ImageFile.FileName);
            string extension = Path.GetExtension(emote.ImageFile.FileName);


            emote.ProfileId = DBManager.GetProfileId(User.Identity.Name);


            using (var memoryStream = new MemoryStream())
            {
                emote.ImageFile.CopyTo(memoryStream);
                var imageAsBytes = memoryStream.ToArray();
                emote.ImageData = imageAsBytes;
            }
            bool success = DBManager.UploadEmote(emote);
            if (success)
            {
                TempData["Message"] = "Emote has been added!";
                RedirectToAction("UploadEmote", "DashBoard");
            }
            return View();
        }

        public IActionResult RemoveEmote(string emoteName, string profileId)
        {
            return RedirectToAction("Index");
        }

    }
}
