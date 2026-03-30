using SQLite;

namespace KapyTask.Database.Tables;

public class KDiscipline
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull] 
    public string Name { get; set; } = null!;
    
    public string? ColorHexValue { get; set; } // color as rgba (color as hex)
    
    [Ignore]
    public Color Color
    {
        get => ColorHexValue is null ? Colors.Gray : Color.FromArgb(ColorHexValue);
        set => ColorHexValue = value.ToHex();
    }
    
    // Starts with '#'. Used to parse pasted text from tg to db. 
    // public string? HashTag { get; set; }
}