using MyOwnDb.Server.Stores.Read;
using MyOwnDb.Server.Stores.Write;

namespace MyOwnDb.Server.Stores;

public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStore(this IEndpointRouteBuilder app) => app
        .MapReadStores()
        .MapWriteToStores();
}

