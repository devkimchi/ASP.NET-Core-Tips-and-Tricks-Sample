using System.Collections.Generic;

namespace AspNetCoreTipsAndTricksSample.Responses
{
    /// <summary>
    /// This represents the response entity for values.
    /// </summary>
    public class ValueResponse
    {
        /// <summary>
        /// Gets or sets the list of values.
        /// </summary>
        public IEnumerable<string> Values { get; set; }
    }
}
