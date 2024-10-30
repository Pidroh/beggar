public class LogUnit 
{
    public LogType logType;

    public RuntimeUnit Unit { get; internal set; }

    public enum LogType 
    { 
        UNIT_UNLOCKED, // When the unit's require is met
        SKILL_IMPROVED,
        CLASS_CHANGE, 

    }
}
