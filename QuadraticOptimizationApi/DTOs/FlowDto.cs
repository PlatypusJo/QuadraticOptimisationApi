namespace QuadraticOptimizationApi.DTOs
{
    public class FlowDto
    {
        public required string Name { get; set; }
        public double MeasuredValue { get; set; }
        public double Tolerance { get; set; }
        public bool IsMeasured { get; set; }
        public required RangeDto MetrologicRange { get; set; }
        public required RangeDto TechnologicRange { get; set; }
    }
}
