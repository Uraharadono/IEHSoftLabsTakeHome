namespace AnalysisEngine.DockerService
{
    public class ContainerInfo
    {
        public string Id { get; set; }
        public List<PortMapping> Ports { get; set; } = new();
    }
    public class PortMapping
    {
        public int PrivatePort { get; set; }
        public int PublicPort { get; set; }
        public string Type { get; set; }
    }
}
