using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{

    public class TasksOperations
    {
        private readonly KapyTaskDatabase _db;

        internal TasksOperations(KapyTaskDatabase db)
        {
            _db = db;
        }

        private async Task<SQLiteAsyncConnection> GetDb()
        {
            await _db.EnsureInitialized();
            return _db.Db;
        }

        public async Task<List<KTask>> GetTasks()
        {
            var db = await GetDb();
            return await db.Table<KTask>().ToListAsync();
        }
        
        public async Task InsertTask(KTask task)
        {
            var db = await GetDb();
            await db.InsertAsync(task);
        }
    }

    private TasksOperations _tasks;
    public TasksOperations Tasks => _tasks ??= new TasksOperations(this);
}