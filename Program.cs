using Muebles_Elohim.Components;
using Muebles_Elohim.Models;
using Supabase;
using Muebles_Elohim.Services;
using System.Text;

namespace Muebles_Elohim
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.Configure<ContactoOpciones>(
            builder.Configuration.GetSection(ContactoOpciones.Seccion));

            builder.Services.AddScoped<ListaCotizacionServicio>();
            builder.Services.AddScoped<CotizacionServicio>();

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CatalogoServicio>();


            // ==========================================
            // Configuración de Supabase
            // ==========================================
            var supabaseUrl = builder.Configuration["Supabase:Url"]
                ?? throw new InvalidOperationException("Falta Supabase:Url");

            var supabaseKey = builder.Configuration["Supabase:Key"]
                ?? throw new InvalidOperationException("Falta Supabase:Key");

            // Registramos el cliente de Supabase como Singleton para usarlo en toda la app
            builder.Services.AddSingleton(_ =>
                new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false // Desactivado para ahorrar recursos si no usas WebSockets
                })
            );
            // ==========================================

            var app = builder.Build();

            var supabase = app.Services.GetRequiredService<Supabase.Client>();
            try
            {
                await supabase.InitializeAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "No se pudo inicializar Supabase al arrancar");
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.MapGet("/robots.txt", (IConfiguration config) =>
            {
                string sitio = (config["Contacto:SitioUrl"] ?? "").TrimEnd('/');
                return Results.Text(
                    "User-agent: *\nAllow: /\nDisallow: /cotizacion\nSitemap: " + sitio + "/sitemap.xml\n",
                    "text/plain");
            });

            app.MapGet("/sitemap.xml", async (IConfiguration config, CatalogoServicio catalogo) =>
            {
                string sitio = (config["Contacto:SitioUrl"] ?? "").TrimEnd('/');
                if (sitio.Length == 0) return Results.NotFound();

                var rutas = new List<string> { "", "/productos", "/contacto", "/privacidad" };
                foreach (var p in await catalogo.ObtenerAsync())
                {
                    rutas.Add("/productos?id=" + p.Id);
                }

                var xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                xml.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
                foreach (string ruta in rutas)
                {
                    xml.Append("<url><loc>" + sitio + ruta + "</loc></url>\n");
                }
                xml.Append("</urlset>");

                return Results.Text(xml.ToString(), "application/xml");
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}