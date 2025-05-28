using ArtworkStoreApi.Models;

namespace ArtworkStoreApi.DTOs
{
    public class ResultDto<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ResultDto()
        {
        }

        public ResultDto(bool isSuccess, string message, T data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public ResultDto(bool isSuccess, string message, T data, List<string> errors)
            : this(isSuccess, message, data)
        {
            Errors = errors ?? new List<string>();
        }

        // Static factory methods
        public static ResultDto<T> Success(T data, string message = "Operation completed successfully")
        {
            return new ResultDto<T>(true, message, data);
        }

        public static ResultDto<T> Failure(string message, List<string> errors = null)
        {
            return new ResultDto<T>(false, message, default(T), errors);
        }

        public static ResultDto<T> Failure(string message, string error)
        {
            return new ResultDto<T>(false, message, default(T), new List<string> { error });
        }
    }

// Non-generic verze
    public class ResultDto : ResultDto<object>
    {
        public ResultDto() : base()
        {
        }

        public ResultDto(bool isSuccess, string message) : base(isSuccess, message, null)
        {
        }

        public ResultDto(bool isSuccess, string message, List<string> errors) 
            : base(isSuccess, message, null, errors)
        {
        }

        public static ResultDto Success(string message = "Operation completed successfully")
        {
            return new ResultDto(true, message);
        }

        public static new ResultDto Failure(string message, List<string> errors = null)
        {
            return new ResultDto(false, message, errors);
        }

        public static new ResultDto Failure(string message, string error)
        {
            return new ResultDto(false, message, new List<string> { error });
        }
    }
}