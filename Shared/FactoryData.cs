namespace Shared
{
    public class FactoryData
    {
        public Guid ID { get; set; }
        public DateTime Time { get; set; }
        public int Value { get; set; }
    }

    public enum FactoryState
    {
        Running,
        Stopped,
        Broken
    }

    public class FactoryInfo
    {
        public Guid ID { get; set; }
        public double Status { get; set; }
        public FactoryState State { get; set; }
    }
}