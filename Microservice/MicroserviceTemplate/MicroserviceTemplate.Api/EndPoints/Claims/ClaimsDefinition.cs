using System.Security.Claims;
using MicroserviceTemplate.Api.Definitions.Identity;
using MicroserviceTemplate.Base.Attributes;
using MicroserviceTemplate.Base.Definition;
using MicroserviceTemplate.DAL.Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace MicroserviceTemplate.Api.EndPoints.Claims;

public class ClaimsDefinition : Definition
{
    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.MapGet("~/api/claims/get", GetClaims).WithOpenApi();
    }
    
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [FeatureGroupName("Claims")]
    [Authorize(AuthenticationSchemes = AuthData.AuthenticationSchemes)]
    private async Task<IResult> GetClaims( 
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var result = claims.Select(x => new { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        return Results.Ok(result);
    }
}