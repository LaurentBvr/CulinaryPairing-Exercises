using Ardalis.Result.AspNetCore;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using CulinaryPairing.Application.Features.Dishes;
using CulinaryPairing.Application.Features.Pairings;
using CulinaryPairing.Domain.Pairings;

namespace CulinaryPairing.Api.Endpoints;

public static class DishesEndpoint
{
    public static IEndpointRouteBuilder MapDishes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/dishes").WithTags("Dishes");

        // Lecture : accessible à tous les utilisateurs authentifiés
        group.MapGet("/", GetDishes).RequireAuthorization();
        group.MapGet("/{id:guid}", GetDishDetail).RequireAuthorization();
        group.MapGet("/{id:guid}/pairings", GetDishPairings).RequireAuthorization();

        // Création : Admin et FamilyMember
        group.MapPost("/", CreateDish).RequireAuthorization(policy =>
        policy.RequireRole("Admin", "FamilyMember"));
        group.MapPost("/{id:guid}/pairings", CreatePairing).RequireAuthorization(policy =>
        policy.RequireRole("Admin", "FamilyMember"));

        // Validation : Admin uniquement
        group.MapPut("/pairings/{id:guid}/validate", ValidatePairing).RequireAuthorization(policy =>
        policy.RequireRole("Admin"));

        return app;
    }

    internal static async Task<IResult> GetDishes(ISender sender)
    {
        var response = await sender.Send(new GetDishes());
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> GetDishDetail(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetDishDetail(id));
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> CreateDish(
        ISender sender, [FromBody] CreateDish command)
    {
        var response = await sender.Send(command);
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> GetDishPairings(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetDishPairings(id));
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> CreatePairing(
        ISender sender, Guid id, [FromBody] CreatePairingRequest request)
    {
        var command = new CreatePairing(
            Guid.NewGuid(), request.BeverageName, id, request.Score);
        var response = await sender.Send(command);
        return response.ToMinimalApiResult();
    }

    internal static async Task<IResult> ValidatePairing(ISender sender, Guid id)
    {
        var response = await sender.Send(new ValidatePairing(id));
        return response.ToMinimalApiResult();
    }
}

public record CreatePairingRequest(string BeverageName, PairingScore Score = PairingScore.Medium);