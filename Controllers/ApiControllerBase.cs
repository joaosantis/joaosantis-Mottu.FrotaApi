using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mottu.FrotaApi.Controllers
{
    /// <summary>
    /// Base controller com UMA única sobrecarga de Problem compatível com .NET 8
    /// (e que NÃO é tratada como action).
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Monta ProblemDetails com suporte a 'extensions'.
        /// Assinatura pensada para chamadas nomeadas: title, detail, statusCode, type, extensions.
        /// </summary>
        [NonAction] // evita ser exposta como endpoint pelo Swagger
        protected ObjectResult Problem(
            string? title = null,
            string? detail = null,
            int? statusCode = null,
            string? type = null,
            IDictionary<string, object?>? extensions = null)
        {
            var pd = new ProblemDetails
            {
                Title   = title,
                Detail  = detail,
                Status  = statusCode,
                Type    = type,
                Instance = HttpContext?.TraceIdentifier
            };

            if (extensions != null)
            {
                foreach (var kv in extensions)
                    pd.Extensions[kv.Key] = kv.Value!;
            }

            return StatusCode(pd.Status ?? StatusCodes.Status500InternalServerError, pd);
        }
    }
}
