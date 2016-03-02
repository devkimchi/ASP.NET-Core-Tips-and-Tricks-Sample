using System.Collections.Generic;

using Swashbuckle.SwaggerGen;

namespace AspNetCoreTipsAndTricksSample.Filters
{
    /// <summary>
    /// This represents the document filter entity for Swagger document.
    /// </summary>
    public class SchemaDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// Applies filter context to swagger document.
        /// </summary>
        /// <param name="swaggerDoc"><see cref="SwaggerDocument"/> instance.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/> instance.</param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Host = "localhost:44390";
            swaggerDoc.BasePath = "/";
            swaggerDoc.Schemes = new List<string>() { "https" };
        }
    }
}
