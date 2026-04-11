using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{

    public class BaseOperations
    {
        protected readonly KapyTaskDatabase _db;

        internal BaseOperations(KapyTaskDatabase db)
        {
            _db = db;
        }

        protected async Task<SQLiteAsyncConnection> GetDb()
        {
            await _db.EnsureInitialized();
            return _db.Db;
        }

    }
}