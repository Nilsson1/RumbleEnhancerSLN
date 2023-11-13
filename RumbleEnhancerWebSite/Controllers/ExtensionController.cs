using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RumbleEnhancerWebSite.Controllers
{
    public class ExtensionController : Controller
    {
        private readonly IEmoteData _emoteData;
        private readonly IProfileData _profileData; //MAKE 3RD CONTROLLER THAT ONLY HANDLES EXTENSION GET REQUEST ENDPOINT?

        public ExtensionController(IEmoteData emoteData, IProfileData profileData)
        {
            _emoteData = emoteData;
            _profileData = profileData;
        }

        [HttpGet]
        public async Task<string> Index(string name)
        {
            string json = "";
            try
            {
                var profile = await _profileData.GetProfileFromName(name);
                var emotes = await _emoteData.GetEmotes(profile.ProfileId.ToString());
                json = JsonConvert.SerializeObject(emotes);

            }catch (Exception ex)
            {
                //DO SOMETHING
            }
            return json;
        }
    }
}
