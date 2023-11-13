using DataAccessLibrary.Models;

namespace DataAccessLibrary.Data
{
    public interface IEmoteData
    {
        Task InsertEmote(Emote emote);
        Task<List<Emote>> GetEmotes(string id);
        Task<Emote> GetEmote(string id, string emoteName);
        Task RemoveEmote(string id, string emoteName);
    }
}
