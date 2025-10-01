using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mottu.FrotaApi.Utils
{
    // Permite usar Problem(..., extensions: ...) também no .NET 8
    public static class ControllerBaseExtensions
    {
        public static IActionResult Problem(
            this ControllerBase controller,
            string? detail = null,
            string? instance = null,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            IDictionary<string, object>? extensions = null)
        {
            var pd = new ProblemDetails
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type
            };

            if (extensions != null)
            {
                foreach (var kv in extensions)
                    pd.Extensions[kv.Key] = kv.Value;
            }

            // Se não veio status, usa 500 por padrão
            var code = pd.Status ?? StatusCodes.Status500InternalServerError;
            return controller.StatusCode(code, pd);
        }
    }
}
