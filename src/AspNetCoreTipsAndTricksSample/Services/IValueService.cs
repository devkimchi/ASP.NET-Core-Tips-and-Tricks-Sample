using System;

using AspNetCoreTipsAndTricksSample.Responses;

namespace AspNetCoreTipsAndTricksSample.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="ValueService"/> class.
    /// </summary>
    public interface IValueService : IDisposable
    {
        /// <summary>
        /// Gets the list of values.
        /// </summary>
        /// <returns>Returns the <see cref="ValueResponse"/> object.</returns>
        ValueResponse GetValues();
    }
}