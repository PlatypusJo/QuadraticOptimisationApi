using QuadraticOptimizationApi.DTOs;

namespace QuadraticOptimizationApi.RequestModels
{
    public class BalanceRequest
    {
        public required List<NodeDto> Nodes { get; set; }
        public required List<FlowDto> Flows { get; set; }
        public List<ConstraintDto>? Constraints { get; set; }
    }
}