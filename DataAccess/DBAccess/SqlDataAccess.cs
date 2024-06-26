﻿using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Data;

namespace DataAccessLibrary.DBAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<T>> LoadData<T, U>(string sql, U parameters, string connectionId = "DefaultConnectionString")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnectionString"));
            return await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task SaveData<T>(string sql, T parameters, string connectionId = "DefaultConnectionString")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnectionString"));

            await connection.ExecuteAsync(sql, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
