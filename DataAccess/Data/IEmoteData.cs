using DataAccessLibrary.Models;

namespace DataAccessLibrary.Data
{
    public interface IEmoteData
    {
        Task InsertEmote(Emote emote);
        Task<List<Emote>> GetEmotes(string id);
    }
}
