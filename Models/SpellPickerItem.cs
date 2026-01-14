namespace M59AdminTool.Models
{
    public sealed class SpellPickerItem
    {
        public SpellPickerItem(int id, string sidName, string? germanName)
        {
            Id = id;
            SidName = sidName;
            GermanName = germanName;
        }

        public int Id { get; }
        public string SidName { get; }
        public string? GermanName { get; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(GermanName))
                    return $"{Id} - {SidName}";

                return $"{Id} - {SidName} - {GermanName}";
            }
        }
    }
}
