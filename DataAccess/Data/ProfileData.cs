﻿using DataAccessLibrary.DBAccess;
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

        public async Task<Profile?> GetProfileFromEmail(string email)
        {
            var results = await _db.LoadData<Profile, dynamic>(
                "profile_GetProfileFromEmail",
                new { ProfileEmail = email });
            return results.FirstOrDefault();
        }

        public async Task<Profile?> GetProfileFromName(string name)
        {
            var results = await _db.LoadData<Profile, dynamic>(
                "profile_GetProfileFromName",
                new { ProfileName = name });
            return results.FirstOrDefault();
        }

        public Task InsertProfile(Profile profile) =>
            _db.SaveData(
                "dbo.profile_Insert",
                new { profile.ProfileId, profile.ProfileName, profile.ProfilePassword, profile.ProfileEmail, ProfileVerified = 0 });

        public Task SetVerifiedProfile(string profileId) =>
            _db.SaveData(
                "[profile_SetVerified]",
                new { profileId });

        public Task DeleteProfile(int id) =>
            _db.SaveData(
                "test",
                new { ProfileId = id });
    }
}
