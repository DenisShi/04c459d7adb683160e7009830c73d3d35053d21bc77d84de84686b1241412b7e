using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Api.Application;

namespace PhoneBook.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/phone-numbers")]
public sealed class PhoneNumbersController(PhoneNumberService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List phone numbers")]
    [EndpointDescription("Returns the entries visible to the caller, newest first. Scope narrows the result: all, personal or shared.")]
    [ProducesResponseType<IReadOnlyList<PhoneNumberResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<PhoneNumberResponse>>> List(
        [FromQuery] string? scope,
        CancellationToken cancellationToken)
    {
        if (!PhoneNumberScopes.TryParse(scope, out var parsedScope))
        {
            ModelState.AddModelError(nameof(scope), "The scope must be one of: all, personal, shared.");
            return ValidationProblem(ModelState);
        }

        return Ok(await service.ListAsync(parsedScope, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a phone number")]
    [EndpointDescription("Returns one entry. Entries that do not exist and personal entries of other users both produce 404.")]
    [ProducesResponseType<PhoneNumberResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<PhoneNumberResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var response = await service.FindAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Create a phone number")]
    [EndpointDescription("Creates an entry owned by the caller. Entries cannot be changed or removed afterwards.")]
    [ProducesResponseType<PhoneNumberResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    public async Task<ActionResult<PhoneNumberResponse>> Create(
        CreatePhoneNumberRequest request,
        CancellationToken cancellationToken)
    {
        var response = await service.CreateAsync(request, cancellationToken);
        return Created(Url.Action(nameof(Get), new { id = response.Id }), response);
    }
}
