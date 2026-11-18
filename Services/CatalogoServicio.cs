using Microsoft.Extensions.Caching.Memory;
using Muebles_Elohim.Models;

namespace Muebles_Elohim.Services
{
    // Lee el catálogo de Supabase y lo guarda en memoria unos minutos
    public class CatalogoServicio
    {
        private const string LlaveCache = "catalogo_fotos";
        private static readonly TimeSpan Duracion = TimeSpan.FromMinutes(10);

        // Evita que varios visitantes consulten Supabase a la vez cuando el caché está vacío
        private static readonly SemaphoreSlim Candado = new SemaphoreSlim(1, 1);

        private readonly Supabase.Client _supabase;
        private readonly IMemoryCache _cache;

        public CatalogoServicio(Supabase.Client supabase, IMemoryCache cache)
        {
            _supabase = supabase;
            _cache = cache;
        }

        public async Task<List<CatalogoFoto>> ObtenerAsync()
        {
            List<CatalogoFoto>? guardados;

            if (_cache.TryGetValue(LlaveCache, out guardados) && guardados != null)
            {
                return new List<CatalogoFoto>(guardados);
            }

            await Candado.WaitAsync();

            try
            {
                // Otro visitante pudo llenar el caché mientras esperábamos
                if (_cache.TryGetValue(LlaveCache, out guardados) && guardados != null)
                {
                    return new List<CatalogoFoto>(guardados);
                }

                var respuesta = await _supabase.From<CatalogoFoto>().Get();

                List<CatalogoFoto> lista = respuesta.Models
                    .OrderBy(p => p.Id)
                    .ToList();

                // Una lista vacía no se guarda: así un fallo no se queda pegado 10 minutos
                if (lista.Count > 0)
                {
                    _cache.Set(LlaveCache, lista, Duracion);
                }

                return new List<CatalogoFoto>(lista);
            }
            finally
            {
                Candado.Release();
            }
        }
    }
}
