using DataAccessLibrary.DBAccess;
using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Data
{
    public class EmoteData : IEmoteData
    {
        private readonly ISqlDataAccess _db;

        public EmoteData(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<List<Emote>> GetEmotes(string id)
        {
            var results = await _db.LoadData<Emote, dynamic>(
                "emote_GetEmotes",
                new { ProfileId = id });

            return results.ToList();
        }

        public Task InsertEmote(Emote emote) =>
            _db.SaveData(
                "emote_InsertEmote",
                new { emote.ProfileId, emote.EmoteName, emote.ImageData });

    }
}
