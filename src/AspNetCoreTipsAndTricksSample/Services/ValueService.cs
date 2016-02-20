using System.Collections.Generic;

namespace AspNetCoreTipsAndTricksSample.Services
{
    /// <summary>
    /// This represents the service entity for values.
    /// </summary>
    public class ValueService : IValueService
    {
        private bool _disposed;

        /// <summary>
        /// Gets the list of values.
        /// </summary>
        /// <returns>Returns the list of values.</returns>
        public IEnumerable<string> GetValues()
        {
            return new[] { "value1", "value2" };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}