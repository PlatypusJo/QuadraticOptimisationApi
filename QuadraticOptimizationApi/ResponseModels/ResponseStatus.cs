namespace QuadraticOptimizationApi.ResponseModels
{
    public class ResponseStatus<T>
    {
        public ResponseStatus(T value, string status)
        {
            Value = value;
            Status = status;
        }

        public T Value { get; set; }

        public string Status { get; set; }
    }
}
