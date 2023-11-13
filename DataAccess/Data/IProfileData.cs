using DataAccessLibrary.Models;

namespace DataAccessLibrary.Data
{
    public interface IProfileData
    {
        Task DeleteProfile(int id);
        Task<Profile?> GetProfile(string id);
        Task<Profile?> GetProfileFromEmail(string email);
        Task<Profile?> GetProfileFromName(string name);
        Task<IEnumerable<Profile>> GetProfiles();
        Task InsertProfile(Profile profile);
        Task SetVerifiedProfile(string profileId);


    }
}