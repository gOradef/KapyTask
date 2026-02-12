using SQLite;
using KapyTask.Database.Tables;

namespace KapyTask.Database;

public class KapyTaskDatabase
{
    private SQLiteAsyncConnection db;

    async Task Init()
    {
        if (db is not null)
            return;
        
        db = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        var tasks = await db.CreateTableAsync<KTask>();
        var disciplines = await db.CreateTableAsync<KDiscipline>();
    }

    public async Task<string> GetDatabasePath()
    {
        await Init();
        return db.DatabasePath;
    }

    public async Task<List<KTask>> GetAllTasks()
    {
        await Init();
        return await db.Table<KTask>().ToListAsync();
    }

    public async Task<int> InsertTask(KTask task)
    {
        await Init();
        return await db.InsertAsync(task);
    }
}