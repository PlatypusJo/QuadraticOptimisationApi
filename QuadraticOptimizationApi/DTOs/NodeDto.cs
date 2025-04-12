namespace QuadraticOptimizationApi.DTOs
{
    public class NodeDto
    {
        public required string Name { get; set; }
        public required List<string> InputFlows { get; set; }
        public required List<string> OutputFlows { get; set; }
    }
}
