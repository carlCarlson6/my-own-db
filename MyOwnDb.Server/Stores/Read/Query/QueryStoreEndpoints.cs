using Microsoft.AspNetCore.Mvc;

namespace MyOwnDb.Server.Stores.Read.Query;

public static class QueryStoreEndpoints
{
    public static IEndpointRouteBuilder MapQueryStore(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store/{storeName}/query", QueryStore);
        return app;
    }

    private static async Task<IResult> QueryStore(
        [FromRoute(Name = nameof(storeName))] string storeName,
        [FromBody] QueryStoreRequest request,
        [FromServices] QueryBuilder queryBuilder,
        HttpContext ctx)
    {
        // TODO check path does not contain sql
        
        var records = await queryBuilder
            .WithStoreId(new StoreId(storeName, ctx.GetTenantId()))
            .WithFilters(request.Filters.ToArray())
            .Run(ctx.RequestAborted);
        return Results.Ok(records);
    }
}

public record QueryStoreRequest(List<QueryFilterOperation> Filters);

