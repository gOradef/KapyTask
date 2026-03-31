using SQLite;
using static KapyTask.Database.DaysLeftRecord;

namespace KapyTask.Database.Tables;

/// <summary>
/// Stands for KapyTask = KTask
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
    
    /// <summary>
    /// Pretty much unused
    /// </summary>
    public DateTime? UserPlannedTimeTodo { get; set; } // sets user as optionable
    public DateTime? Deadline { get; set; } // gets from Schedule
    
    // Navigation property (not stored)
    [Ignore]
    public KDiscipline? Discipline { get; set; }

    [Ignore]
    // Requires to Deadline property to be set
    public DaysLeftRecord DaysLeft
    {
        get
        {
            var isUserHasSetCustomTimeTodo = UserPlannedTimeTodo is not null;
            var isDeadlineSetted = Deadline is not null;

            switch (isDeadlineSetted)
            {
                case false when !isUserHasSetCustomTimeTodo:
                    return new("Не установлено", Colors.Gray);
                case true when !isUserHasSetCustomTimeTodo:
                    return CreateDaysLeftRecord((DateTime)Deadline!);
            }

            var result = CreateDaysLeftRecord((DateTime)UserPlannedTimeTodo!);
            result = result with { DaysLeftText = result.DaysLeftText + " (польз.)" };
            return result;
        }
    }

    [Ignore] 
    public string? DayOfWeekShortName => Deadline?.ToString("ddd");

    private DaysLeftRecord CreateDaysLeftRecord(DateTime DateInput)
    {
        var days = ((TimeSpan)(DateInput - DateTime.Today)).Days;
        return days switch
        {
            < 0 => new("Просрочено", Colors.IndianRed),
            0 => new("Сегодня", Colors.IndianRed),
            >= 4 => new($"Через {days}д.", Colors.CornflowerBlue),
            >= 2 => new($"Осталось {days}д.", Color.FromRgb(233, 213, 2)),
            1 => new("Завтра", Color.FromRgb(233, 213, 2)),
        };
    }

    [Ignore] 
    public bool IsHasDescription => Description is not null && Description != "";
}