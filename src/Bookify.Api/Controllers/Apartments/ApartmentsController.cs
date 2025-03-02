﻿using Asp.Versioning;
using Bookify.Application.Apartments.SearchApartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments;

[Authorize]
[Route("api/v{version:apiVersion}/apartments")]
[ApiVersion(ApiVersions.V1)]
[ApiController]
public class ApartmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchApartments(DateOnly startDate, DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var query = new SearchApartmentsQuery(startDate, endDate);

        var result = await sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }
}