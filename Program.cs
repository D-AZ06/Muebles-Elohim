using Supabase;
using Muebles_Elohim.Components;

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

            // ==========================================
            // Configuración de Supabase
            // ==========================================
            var supabaseUrl = builder.Configuration["Supabase:Url"];
            var supabaseKey = builder.Configuration["Supabase:Key"];

            // Registramos el cliente de Supabase como Singleton para usarlo en toda la app
            builder.Services.AddSingleton(_ =>
                new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false // Desactivado para ahorrar recursos si no usas WebSockets
                })
            );
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