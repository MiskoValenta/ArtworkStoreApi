using ArtworkStoreApi.Data;

namespace ArtworkStoreApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connString = builder.Configuration.GetConnectionString("ArtworkStoreApi");
            builder.Services.AddSqlite<DatabaseContext>(connString);

            var app = builder.Build();

            /*app.MapGet("/", () => "Hello World!");*/
            app.MapArtworksController();
            app.MapAdminController();
            app.MapGenresController();
            app.MapOrdersController();
            app.MapReviewsController();

            // na každou service pøidat Scoped
            // Registrovat Logger a pøes DI implementovat
            // appsettings.json


            app.Run();
        }
    }
}
