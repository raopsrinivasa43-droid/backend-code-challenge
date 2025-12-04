using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Logic;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result switch
        {
            // GetAll / GetById
            Success<Message> successSingle => new OkObjectResult(successSingle.Value),
            Success<IEnumerable<Message>> successList => new OkObjectResult(successList.Value),

            // Create
            Created<Message> created => new CreatedResult(string.Empty, created.Value),

            // Update / Delete
            Updated => new NoContentResult(),
            Deleted => new NoContentResult(),

            // Errors
            NotFound notFound => new NotFoundObjectResult(notFound.Message),
            Conflict conflict => new ConflictObjectResult(conflict.Message),
            ValidationError validation => new BadRequestObjectResult(validation.Errors),

            _ => new StatusCodeResult(500)
        };
    }
}
