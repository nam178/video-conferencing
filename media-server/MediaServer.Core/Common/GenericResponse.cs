namespace MediaServer.Core.Common
{
    public class GenericResponse
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public static GenericResponse SuccessResponse() 
            => new GenericResponse { Success = true };

        public static GenericResponse ErrorResponse(string errorMessage) 
            => new GenericResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };

        public static GenericResponse InternalServerErrorResponse(string errorMessage) 
            => ErrorResponse($"Internal Server Error: {errorMessage}");
    }
}