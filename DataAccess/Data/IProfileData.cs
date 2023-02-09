﻿using DataAccessLibrary.Models;

namespace DataAccessLibrary.Data
{
    public interface IProfileData
    {
        Task DeleteProfile(int id);
        Task<Profile?> GetProfile(string id);
        Task<IEnumerable<Profile>> GetProfiles();
        Task InsertProfile(Profile profile);
        Task SetVerifiedProfile(Profile profile);
    }
}