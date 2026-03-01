using SQLite;

namespace KapyTask.Database.Tables;

/// <summary>
/// For KapyTask = KTask
/// </summary>
public class KTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Store only the Discipline ID
    [Indexed]
    public int? DisciplineId { get; set; }
    
    [NotNull]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public DateTime? PlannedTimeTodo { get; set; } // sets user as option
    public DateTime? Deadline { get; set; } // gets from Discipline
    
    // Navigation property (not stored)
    [Ignore]
    public KDiscipline? Discipline { get; set; }
}