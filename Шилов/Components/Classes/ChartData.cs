namespace Шилов.Components.Classes
{
    public class ChartData
    {
        public string Category { get; set; } = string.Empty;
        public int Value { get; set; }

        public ChartData(string cat, int val)
        {
            this.Category = cat;
            this.Value = val;
        }
    }
}
