using KapyTask.Database.Tables;
using SQLite;
using System.Threading.Tasks;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{

    public class ArchivedTasksOperations(KapyTaskDatabase db) : BaseOperations(db)
    {
        public async Task<List<ArchivedKTask>> GetArchivedTasks()
        {
            var db = await GetDb();
            return await db.Table<ArchivedKTask>().ToListAsync();
        }

        public async Task<List<ArchivedKTask>> GetArchivedTasksWithDisciplineProperty()
        {
            var db = await GetDb();
            var tasks = await db.Table<ArchivedKTask>().ToListAsync();
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
        public async Task InsertArchivedTask(ArchivedKTask task)
        {
            var db = await GetDb();
            
            if (task.Id == 0)
                await db.InsertAsync(task);
            else 
                await db.UpdateAsync(task);
        }

        public async void DeleteArchivedTask(ArchivedKTask task)
        {
            var db = await GetDb();
            await db.DeleteAsync(task);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task">Uses KTASK!</param>
        public async Task ArchiveTask(KTask task)
        {
            var db = await GetDb();
            var archivedTask = new ArchivedKTask(task);
            await db.InsertAsync(archivedTask);
            await db.DeleteAsync(task);
        }

        public async Task<int> ClearWholeArchive()
        {
            var db = await GetDb();
            var deleted = await db.DeleteAllAsync<ArchivedKTask>();
            return deleted;
        }
    }

    private ArchivedTasksOperations _archivedTasks;
    public ArchivedTasksOperations ArchivedTasks => _archivedTasks ??= new ArchivedTasksOperations(this);
}