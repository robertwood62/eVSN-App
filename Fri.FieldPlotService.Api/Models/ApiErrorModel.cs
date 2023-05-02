using Fri.FieldPlotService.Tools;

namespace Fri.FieldPlotService.Api.Models
{
    /// <summary>
    /// Model that represents error from the API.
    /// </summary>
    public class ApiErrorModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception"></param>
        public ApiErrorModel(FieldPlotServiceException exception)
        {
            ErrorCode = (int)exception.ErrorCode;
            Message = exception.Message;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception"></param>
        public ApiErrorModel(Exception exception)
        {
            if(exception is FieldPlotServiceException exception1)
            {
                ErrorCode = (int)exception1.ErrorCode;
            }
            else
            {
                ErrorCode = (int)FieldPlotServiceError.GeneralError;
            }
            Message = exception.Message;
        }

        /// <summary>
        /// Error code.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }
    }
}
