using System.Text.Json;
using KapyTask.Database.Tables;
using SQLite;

namespace KapyTask.Database;

public partial class KapyTaskDatabase
{
    private ConfigOperations _config;
    public ConfigOperations Config => _config ??= new ConfigOperations(this);
    
    /// <summary>
    /// Used to export and import user's config.
    /// </summary>
    public class ConfigOperations
    {
        private readonly KapyTaskDatabase _db;

        internal ConfigOperations(KapyTaskDatabase db)
        {
            _db = db;
        }
        
        private async Task<SQLiteAsyncConnection> GetDb()
        {
            await _db.EnsureInitialized();
            return _db.Db;
        }

        public async void BackupDatabase()
        {
            var db = await GetDb();
            await db.BackupAsync("test.sqlite");
        }
        
        /// <summary>
        /// Экспорт только таблиц Disciplines и Schedule в JSON
        /// </summary>
        public async Task<string> ExportDisciplinesAndScheduleAsync()
        {
            var db = await GetDb();
            var exportData = new
            {
                Disciplines = await db.Table<KDiscipline>().ToListAsync(),
                Schedule = await db.Table<KSchedule>().ToListAsync(),
                ExportDate = DateTime.Now,
                Version = "1.0"
            };

            return JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Альтернативный метод импорта через InsertOrReplace (проще, но может быть медленнее)
        /// </summary>
        public async Task ImportDisciplinesAndScheduleSimpleAsync(string jsonData)
        {
            var db = await GetDb();
            
            try
            {
                var importData = JsonSerializer.Deserialize<ImportData>(jsonData);
                
                if (importData == null)
                    throw new Exception("Неверный формат данных");

                await db.RunInTransactionAsync(async Task =>
                {
                    // Отключаем внешние ключи
                    await db.ExecuteAsync("PRAGMA foreign_keys = OFF;");
                    
                    try
                    {
                        // Очищаем таблицы
                        await db.DeleteAllAsync<KDiscipline>();
                        await db.DeleteAllAsync<KSchedule>();
                        
                        // Используем InsertOrReplace для сохранения ID
                        if (importData.Disciplines?.Any() == true)
                        {
                            foreach (var discipline in importData.Disciplines)
                            {
                                #if DEBUG 
                                System.Diagnostics.Debug.WriteLine($"Import: {discipline.Name} -> Color: {discipline.ColorHexValue}");
                                #endif
                                await db.InsertOrReplaceAsync(discipline);
                            }
                        }
                        
                        if (importData.Schedule?.Any() == true)
                        {
                            foreach (var schedule in importData.Schedule)
                            {
                                await db.InsertOrReplaceAsync(schedule);
                            }
                        }
                    }
                    finally
                    {
                        // Включаем внешние ключи
                        await db.ExecuteAsync("PRAGMA foreign_keys = ON;");
                    }
                });
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка парсинга JSON: {ex.Message}");
            }
        }

        // Вспомогательный класс для десериализации
        private class ImportData
        {
            public List<KDiscipline>? Disciplines { get; set; }
            public List<KSchedule>? Schedule { get; set; }
            public DateTime ExportDate { get; set; }
            public string? Version { get; set; }
        }
    }
}