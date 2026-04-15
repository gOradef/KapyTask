using SQLite;
using static KapyTask.Database.DaysLeftRecord;

namespace KapyTask.Database.Tables;

/// <summary>
/// Stands for KapyTask = KTask
/// </summary>
public class ArchivedKTask : KTask
{
    [PrimaryKey, AutoIncrement]
    public new int Id { get; set; }

    // Store only the Discipline ID
    [Indexed]
    public new int? DisciplineId { get; set; }
    
    [NotNull]
    public new string Name { get; set; } = string.Empty;
    
    public new string? Description { get; set; }
    
    /// <summary>
    /// Pretty much unused
    /// </summary>
    // public new DateTime? UserPlannedTimeTodo { get; set; } // sets user as optionable
    public new DateTime? Deadline { get; set; } // gets from Schedule
    
    // Navigation property (not stored)
    [Ignore]
    public new KDiscipline? Discipline { get; set; }

    [Ignore]
    // Requires to Deadline property to be set
    public new DaysLeftRecord DaysLeft
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
    public new string? DayOfWeekShortName => Deadline?.ToString("ddd");

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
    public new bool IsHasDescription => Description is not null && Description != "";
    
    public ArchivedKTask() {}
    public ArchivedKTask(KTask task)
    {
        // Copy all properties from KTask
        Id = task.Id;
        Name = task.Name;
        Description = task.Description;
        Deadline = task.Deadline;
        DisciplineId = task.DisciplineId;
    }
}