namespace Sassa.BRM.ViewModels
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
#nullable enable
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
#nullable disable
        public ApiResponse()
        {
            Success = true;
        }
    }
}
