using Muebles_Elohim.Components;
using MongoDB.Driver;

namespace Muebles_Elohim
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // DB
            var mongoConnectionString = builder.Configuration["MongoDBSettings:ConnectionString"];
            var mongoDatabaseName = builder.Configuration["MongoDBSettings:DatabaseName"];

            // Registramos el cliente de MongoDB
            builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

            // Registramos la base de datos para poder inyectarla en cualquier componente .razor
            builder.Services.AddScoped(sp => {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDatabaseName);
            });
            // ==========================================

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
