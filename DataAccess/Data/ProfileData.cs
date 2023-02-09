using DataAccessLibrary.DBAccess;
using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Data
{
    public class ProfileData : IProfileData
    {
        private readonly ISqlDataAccess _db;

        public ProfileData(ISqlDataAccess db)
        {
            _db = db;
        }

        public Task<IEnumerable<Profile>> GetProfiles() =>
            _db.LoadData<Profile, dynamic>("test", new { });

        public async Task<Profile?> GetProfile(string id)
        {
            var results = await _db.LoadData<Profile, dynamic>(
                "test",
                new { ProfileId = id });

            return results.FirstOrDefault();
        }

        public Task InsertProfile(Profile profile) =>
            _db.SaveData(
                "test",
                new { profile.ProfileName, profile.ProfilePassword, profile.ProfileEmail, ProfileVerified = 0 });

        public Task SetVerifiedProfile(Profile profile) =>
            _db.SaveData(
                "test",
                new { ProfileVerified = 1 });

        public Task DeleteProfile(int id) =>
            _db.SaveData(
                "test",
                new { ProfileId = id });
    }
}
