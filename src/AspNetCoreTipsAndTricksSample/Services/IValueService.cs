using System;
using System.Collections.Generic;

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
        /// <returns>Returns the list of values.</returns>
        IEnumerable<string> GetValues();
    }
}