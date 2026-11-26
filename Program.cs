using Muebles_Elohim.Components;
using Muebles_Elohim.Models;
using Supabase;
using Muebles_Elohim.Services;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

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
            builder.Services.AddScoped<FavoritosServicio>();
            builder.Services.AddScoped<CotizacionServicio>();
            builder.Services.AddScoped<EstadoAnimacionServicio>();

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CatalogoServicio>();
            builder.Services.AddScoped<MaterialesServicio>();


            // ==========================================
            // Configuraci�n de Supabase
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

            // Detr�s de un proxy (hosting) la IP real viene en X-Forwarded-For
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                o.KnownNetworks.Clear();
                o.KnownProxies.Clear();
            });

            // L�mite de peticiones HTTP por IP (los archivos est�ticos no cuentan)
            builder.Services.AddRateLimiter(o =>
            {
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ctx.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            var app = builder.Build();

            app.UseForwardedHeaders();


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

                var rutas = new List<string> { "", "/productos", "/materiales", "/contacto", "/privacidad" };
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

            // Encabezados de seguridad
            app.Use(async (ctx, next) =>
            {
                var h = ctx.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                // En desarrollo no se aplica para no romper el hot reload
                if (!app.Environment.IsDevelopment())
                {
                    h["Content-Security-Policy"] =
                        "default-src 'self'; " +
                        "script-src 'self' https://cdn.jsdelivr.net; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                        "font-src 'self' https://cdn.jsdelivr.net; " +
                        "img-src 'self' data: https:; " +
                        "connect-src 'self' wss:; " +
                        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
                }

                await next();
            });

            app.UseStaticFiles();
            app.UseRateLimiter();

            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}