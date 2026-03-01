using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{
    public class DisciplineOperations
    {
        private readonly KapyTaskDatabase _database;
        
        internal DisciplineOperations(KapyTaskDatabase database)
        {
            _database = database;
        }
        
        private async Task<SQLiteAsyncConnection> GetDb()
        {
            await _database.EnsureInitialized();
            return _database.Db;
        }

        public async Task<List<KDiscipline>> GetDisciplines()
        {
            var db = await GetDb();
            return await db.Table<KDiscipline>().ToListAsync();
        }

        public async Task InsertDiscipline(KDiscipline item)
        {
            var db = await GetDb();
            var disciplines = await GetDisciplines();
            if (disciplines.Contains(item))
                return;
            else
                await db.InsertAsync(item, typeof(KDiscipline));
        }

        public async Task DeleteDiscipline(KDiscipline item)
        {
            var db = await GetDb();
            await db.DeleteAsync(item);
        }
    }

    private DisciplineOperations _disciplines;
    public DisciplineOperations Disciplines => _disciplines ??= new DisciplineOperations(this);
}