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

        public async Task<List<KTask>> GetTasksWithDisciplineProperty()
        {
            var db = await GetDb();
            var tasks = await db.Table<KTask>().ToListAsync();
            var disciplines = await db.Table<KDiscipline>().ToListAsync();
            return tasks.Join(disciplines, task => task.DisciplineId, discipline => discipline.Id,
                (task, discipline) =>
                {
                    task.Discipline = discipline;
                    return task;
                }).ToList();
        }
        
        /// <summary>
        /// Create new task if id is 0, and updates table if not
        /// </summary>
        /// <param name="task">Task to insert or update</param>
        public async Task InsertTask(KTask task)
        {
            var db = await GetDb();
            
            if (task.Id == 0)
                await db.InsertAsync(task);
            else 
                await db.UpdateAsync(task);
        }

        public async void DeleteTask(KTask task)
        {
            var db = await GetDb();
            await db.DeleteAsync(task);
        }
    }

    private TasksOperations _tasks;
    public TasksOperations Tasks => _tasks ??= new TasksOperations(this);
}