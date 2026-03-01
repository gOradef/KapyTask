using SQLite;

namespace KapyTask.Database.Tables;

public class KSchedule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [NotNull, Indexed]
    public int DisciplineId { get; set; }
    
    public long TimeOfClassTicks { get; set; } // Store as ticks
    
    public KDayOfWeek DayOfWeek { get; set; }
    public bool IsEvenWeek { get; set; }
    
    [Ignore]
    public TimeOnly TimeOfClass
    {
        get => TimeOnly.FromTimeSpan(TimeSpan.FromTicks(TimeOfClassTicks));
        set => TimeOfClassTicks = value.ToTimeSpan().Ticks;
    }
    [Ignore]
    public string DisciplineName { get; set; }
}