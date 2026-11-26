using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Muebles_Elohim.Components;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;

namespace Muebles_Elohim
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<Routes>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            var contacto = new ContactoOpciones();
            builder.Configuration.GetSection(ContactoOpciones.Seccion).Bind(contacto);
            builder.Services.AddSingleton(Options.Create(contacto));

            builder.Services.AddScoped<ListaCotizacionServicio>();
            builder.Services.AddScoped<FavoritosServicio>();
            builder.Services.AddScoped<CotizacionServicio>();
            builder.Services.AddScoped<EstadoAnimacionServicio>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CatalogoServicio>();
            builder.Services.AddScoped<MaterialesServicio>();

            var url = builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("Falta Supabase:Url");
            var key = builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("Falta Supabase:Key");
            var supabase = new Supabase.Client(url, key, new Supabase.SupabaseOptions { AutoConnectRealtime = false });
            builder.Services.AddSingleton(supabase);

            var host = builder.Build();

            try { await supabase.InitializeAsync(); }
            catch (Exception ex) { Console.WriteLine("No se pudo inicializar Supabase: " + ex.Message); }

            await host.RunAsync();
        }
    }
}