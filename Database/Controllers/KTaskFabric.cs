namespace KapyTask.Database.Controllers;

public class KTaskDefaultsScheme(int? disciplineId)
{
    public int? DisciplineId { get; set; } = disciplineId;
    
}

public class KTaskFabric
{
    KTaskFabric(KTaskDefaultsScheme scheme)
    {
        
    }
}