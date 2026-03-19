using System.ComponentModel;
using System.Reflection;

namespace KapyTask.Database;

/// <summary>
/// Basic System.DayOfWeek but monday as 1st element
/// </summary>
public enum KDayOfWeek
{
    [Description("Понедельник")]
    Monday = 0,
    
    [Description("Вторник")]
    Tuesday = 1,
    
    [Description("Среда")]
    Wednesday = 2,
    
    [Description("Четверг")]
    Thursday = 3,
    
    [Description("Пятница")]
    Friday = 4,
    
    [Description("Суббота")]
    Saturday = 5,
    
    [Description("Воскресенье")]
    Sunday = 6,
}
public static class KDayOfWeekExtensions
{
    public static DayOfWeek ToSystemDayOfWeek(this KDayOfWeek kDay)
    {
        return kDay switch
        {
            KDayOfWeek.Monday => DayOfWeek.Monday,
            KDayOfWeek.Tuesday => DayOfWeek.Tuesday,
            KDayOfWeek.Wednesday => DayOfWeek.Wednesday,
            KDayOfWeek.Thursday => DayOfWeek.Thursday,
            KDayOfWeek.Friday => DayOfWeek.Friday,
            KDayOfWeek.Saturday => DayOfWeek.Saturday,
            KDayOfWeek.Sunday => DayOfWeek.Sunday,
            _ => DayOfWeek.Monday
        };
    }
}