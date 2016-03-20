#if !DEBUG
using Newtonsoft.Json;
#endif

namespace AspNetCoreTipsAndTricksSample.Responses
{
    /// <summary>
    /// This represents the response entity for error.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace.
        /// </summary>
#if !DEBUG
        [JsonIgnore]
#endif
        public string StackTrace { get; set; }
    }
}