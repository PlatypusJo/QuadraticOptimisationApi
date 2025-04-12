namespace QuadraticOptimizationApi.DTOs
{
    public class ConstraintDto
    {
        public string? Type { get; set; }
        public string? Flow1 { get; set; }
        public string? Flow2 { get; set; }
        public double Ratio { get; set; }
    }
}
