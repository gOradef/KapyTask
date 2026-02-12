using SQLite;

namespace KapyTask.Database.Tables;

public class KDiscipline
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
}