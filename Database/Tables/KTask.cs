using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

namespace KapyTask.Database.Tables;

/// <summary>
/// For KapyTask = KTask
/// </summary>
public class KTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int? DisciplineId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    public DateTime? PlannedTimeTodo { get; set; }
    public DateTime? Deadline { get; set; }
    
    public KTask() {}

    public KTask(string name)
    {
        Name = name;
    }
}