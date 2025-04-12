namespace QuadraticOptimizationApi.DTOs
{
    public class FlowDto
    {
        public required string Name { get; set; }
        public double MeasuredValue { get; set; }
        public double Tolerance { get; set; }
    }
}
