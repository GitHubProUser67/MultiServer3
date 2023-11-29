namespace SRVEmu.DataStore
{
    public interface IDatabase
    {
        bool CreateNew(DbAccount info);
        DbAccount? GetByName(string? username);
        int AddPersona(int id, string persona);
        int DeletePersona(int id, string persona);
    }
}
