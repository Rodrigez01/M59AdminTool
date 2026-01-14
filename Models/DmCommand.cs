namespace M59AdminTool.Models
{
    /// <summary>
    /// Represents a DM (Dungeon Master) admin command
    /// </summary>
    public class DmCommand
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool RequiresParameter { get; set; }
        public string? ParameterName { get; set; }
        public string? ParameterDefaultValue { get; set; }
    }

    /// <summary>
    /// Represents music tracks for the DJ system
    /// </summary>
    public class MusicTrack
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TrackId { get; set; }
    }

    /// <summary>
    /// Represents arena/event commands
    /// </summary>
    public class ArenaCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
    }
}
