namespace Cardápio.Infra.Helpers
{
    public class ErrorResponse : Exception
    {
        public string MessageError { get; set; }
        public int CodeError { get; set; }

        public ErrorResponse(string messageError, int codeError)
        {
            MessageError = messageError;
            CodeError = codeError;
        }
    }

    public class ErrorResponseDTO
    {
        public string MessageError { get; set; }
        public int CodeError { get; set; }

        public ErrorResponseDTO(ErrorResponse ex)
        {
            MessageError = ex.MessageError;
            CodeError = ex.CodeError;
        }
    }
}
