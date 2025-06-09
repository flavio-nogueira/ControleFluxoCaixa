using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleFluxoCaixa.Application.Filters
{
   
    /// <summary>
    /// Filtro que intercepta erros de ModelState (validação de modelo JSON).
    /// </summary>
    public class ModelStateValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var erros = context.ModelState
                    .Where(e => e.Value!.Errors.Count > 0)
                    .Select(e => new
                    {
                        Campo = e.Key,
                        Mensagens = e.Value!.Errors.Select(er => er.ErrorMessage)
                    });

                context.Result = new BadRequestObjectResult(new
                {
                    Sucesso = false,
                    Erros = erros
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }

}
