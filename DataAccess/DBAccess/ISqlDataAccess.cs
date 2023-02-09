namespace DataAccessLibrary.DBAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> LoadData<T, U>(string sql, U parameters, string connectionId = "Default");
        Task SaveData<T>(string sql, T parameters, string connectionId = "Default");
    }
}