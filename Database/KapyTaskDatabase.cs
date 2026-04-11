using SQLite;
using KapyTask.Database.Tables;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{
    protected internal SQLiteAsyncConnection Db { get; private set; }

    private async Task Init()
    {
        if (Db is not null)
            return;
        
        Db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await Db.CreateTableAsync<KDiscipline>();
        await Db.CreateTableAsync<KTask>();
        await Db.CreateTableAsync<ArchivedKTask>();
        await Db.CreateTableAsync<KSchedule>();
    }
    
    protected internal async Task EnsureInitialized()
    {
        if (Db is null)
            await Init();
    }

    public async Task<List<KDiscipline>> GetDisciplines()
    {
        await EnsureInitialized();
        return await Db.Table<KDiscipline>().ToListAsync();
    }

    public async Task InsertDiscipline(KDiscipline discipline)
    {
        await EnsureInitialized();
        await Db.InsertAsync(discipline);
    }
}