using System;
using System.Collections.Generic;

using AspNetCoreTipsAndTricksSample.Services;

using Microsoft.AspNet.Mvc;

namespace AspNetCoreTipsAndTricksSample.Controllers
{
    /// <summary>
    /// This represents the controller entity for values.
    /// </summary>
    [Route("values")]
    public class ValuesController : Controller
    {
        private readonly IValueService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValuesController"/> class.
        /// </summary>
        /// <param name="service"><see cref="IValueService"/> instance.</param>
        public ValuesController(IValueService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            this._service = service;
        }

        /// <summary>
        /// Gets the list of values.
        /// </summary>
        /// <returns>Returns the list of values.</returns>
        [HttpGet]
        [Route("")]
        [Produces(typeof(IEnumerable<string>))]
        public IActionResult Get()
        {
            var result = this._service.GetValues();
            return this.Ok(result);
        }
    }
}