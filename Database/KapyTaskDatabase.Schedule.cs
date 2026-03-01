// KapyTaskDatabase.Schedule.cs - Schedule operations file
using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{
    public class ScheduleOperations
    {
        private readonly KapyTaskDatabase _database;
        
        internal ScheduleOperations(KapyTaskDatabase database)
        {
            _database = database;
        }
        
        private async Task<SQLiteAsyncConnection> GetDb()
        {
            await _database.EnsureInitialized();
            return _database.Db;
        }
        
        /// <summary>
        /// Get schedule of user 
        /// </summary>
        /// <returns>Schedule list for odd and even week</returns>
        public async Task<List<KSchedule>> GetSchedule()
        {
            var db = await GetDb();
            return await db.Table<KSchedule>().ToListAsync();
        }
        
        /// <summary>
        /// Get schedule for a specific day
        /// </summary>
        public async Task<List<KSchedule>> GetScheduleForDay(KDayOfWeek day)
        {
            var db = await GetDb();
            return await db.Table<KSchedule>()
                .Where(s => s.DayOfWeek == day)
                .ToListAsync();
        }
        
        /// <summary>
        /// Get schedule
        /// </summary>
        public async Task<List<KSchedule>> GetScheduleForWeek(bool isOddWeek)
        {
            var db = await GetDb();
            return await db.Table<KSchedule>()
                .Where(s => s.IsEvenWeek == isOddWeek)
                .ToListAsync();
        }
    
        /// <summary>
        /// Updates schedule of user
        /// </summary>
        /// <param name="scheduleNew"></param>
        // public async Task UpdateSchedule(KSchedule scheduleNew)
        // {
        //     var db = await GetDb();
        //     await db.UpdateAsync(scheduleNew);
        // }
        
        /// <summary>
        /// Insert new schedule item
        /// </summary>
        /// <remarks> Straight insert </remarks>
        public async Task InsertSchedule(KSchedule scheduleItem)
        {
            var db = await GetDb();
            await db.InsertAsync(scheduleItem);
        }
        
        /// <summary>
        /// Delete schedule item
        /// </summary>
        public async Task DeleteSchedule(KSchedule scheduleItem)
        {
            var db = await GetDb();
            await db.DeleteAsync(scheduleItem);
        }
    }

    private ScheduleOperations _schedule;
    public ScheduleOperations Schedule => _schedule ??= new ScheduleOperations(this);
}