namespace Shared
{
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