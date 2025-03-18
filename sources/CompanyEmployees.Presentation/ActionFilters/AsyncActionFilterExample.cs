using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.Presentation.ActionFilters;

public class AsyncActionFilterExample : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // execute any code before the action executes
        // var result = await next();         
        // execute any code after the action executes
    }
}
