using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using NuGet.Protocol;
using RumbleEnhancerWebSite.Models;
using System.Data;
using System.Drawing;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;

namespace RumbleEnhancerWebSite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IEmoteData _data;

        public DashboardController(IEmoteData data)
        {
            _data = data;
        }

        public async Task<IActionResult> Index()
        {
            DashboardModel dashboard = new DashboardModel();
            var emotes = await _data.GetEmotes(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Copy db data to EmoteModel
            foreach(var emote in emotes)
            {
                dashboard.Emotes.Add(new EmoteModel() { EmoteName = emote.EmoteName, ProfileId = emote.ProfileId, ImageData = emote.ImageData });
            }

            //Add image file to model
            foreach (var emote in dashboard.Emotes)
            {
                using (var ms = new MemoryStream(emote.ImageData))
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

        public async Task<IActionResult> EmoteInfo(string profileId, string emoteName)
        {
            EmoteModel em = new EmoteModel();
            try
            {
                var emote = await _data.GetEmote(profileId, emoteName);

                em.ProfileId = new Guid(profileId);
                em.EmoteName = emoteName;
                em.ImageData = emote.ImageData;
            }
            catch (Exception ex)
            {
                //TODO
            }
            return View(em);
        }

        [HttpPost]
        public async Task<IActionResult> UploadEmote(EmoteModel emote)
        {

            //>>>>>>>>>>>>>>>>TODO: CREATE PROFILE PREFIX FOR EMOTES TO PREVENT DUPLICATE EMOTENAMES!

            string filename = Path.GetFileNameWithoutExtension(emote.ImageFile.FileName);
            string extension = Path.GetExtension(emote.ImageFile.FileName);

            emote.ProfileId = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using (var memoryStream = new MemoryStream())
            {
                emote.ImageFile.CopyTo(memoryStream);
                var imageAsBytes = memoryStream.ToArray();
                emote.ImageData = imageAsBytes;
            }
            Emote e = new Emote()
            {
                ProfileId = emote.ProfileId,
                EmoteName = emote.EmoteName,
                ImageData = emote.ImageData
            };

            try
            {
                var emoteReply = await _data.GetEmote(e.ProfileId.ToString(), e.EmoteName);
                if (emoteReply != null)
                    throw new Exception();

                await _data.InsertEmote(e);
                TempData["Success"] = "Emote uploaded succesfully!";
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Emote upload failed!");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEmote(string emoteName)
        {
            try
            {
                await _data.RemoveEmote(User.FindFirstValue(ClaimTypes.NameIdentifier), emoteName);
            }
            catch(Exception ex)
            {
                //TODO
            }

            return RedirectToAction("Index");
        }
    }
}
