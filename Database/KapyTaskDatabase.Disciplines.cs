using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{
    private DisciplineOperations _disciplines;
    public DisciplineOperations Disciplines => _disciplines ??= new DisciplineOperations(this);

    public class DisciplineOperations(KapyTaskDatabase db) : BaseOperations(db)
    {
        public async Task<List<KDiscipline>> GetDisciplines()
        {
            var db = await GetDb();
            return await db.Table<KDiscipline>().ToListAsync();
        }

        /// <summary>
        /// Insert discipline to db, but NOT UPDATE ANY. For update purpose use <see cref="UpdateDiscipline"/>
        /// </summary>
        /// <exception cref="item"> If exists in db. Throws <see cref="ArgumentException"/></exception>
        public async Task InsertDiscipline(KDiscipline item)
        {
            var db = await GetDb();
            var disciplines = await GetDisciplines();
            if (disciplines.Contains(item))
            {
                throw new ArgumentException("Дисциплина уже добавлена.");
            }

            await db.InsertAsync(item, typeof(KDiscipline));
        }
        /// <summary>
        /// Update discipline in db. />
        /// </summary>
        /// <exception cref="item"> If doesnt exists in db. Throws <see cref="ArgumentException"/></exception>

        public async Task UpdateDiscipline(KDiscipline item)
        {
            var db = await GetDb();
            var disciplines = await GetDisciplines();
            if (disciplines.Exists(a => a.Id == item.Id))
            {
                await db.UpdateAsync(item, typeof(KDiscipline));
                return;
            }

            throw new ArgumentException("Дисциплины нет в базе данных. Обновление невозможно.");
        }

        public async Task DeleteDiscipline(KDiscipline item)
        {
            var db = await GetDb();
            await db.DeleteAsync(item);
        }
    }
}