using SQLite;

namespace KapyTask.Database.Tables;

public class KDiscipline //todo add hashtag to add task from tg message from Bagir)) 
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull] 
    public string Name { get; set; } = null!;
    
    // Starts with '#'. Used to parse pasted text from tg to db. 
    public string? HashTag { get; set; }
}