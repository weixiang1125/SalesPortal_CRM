namespace IOStandard
{
    public class ActionResults<T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public static ActionResults<T> Success(T data = default)
        {
            return new ActionResults<T>
            {
                Data = data,
                IsSuccess = true,
                Message = ""
            };
        }

        public static ActionResults<T> Fail(string message, T data = default)
        {
            return new ActionResults<T>
            {
                Data = data,
                IsSuccess = false,
                Message = message
            };
        }

    }
}