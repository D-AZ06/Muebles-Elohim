using Microsoft.Extensions.Caching.Memory;
using Muebles_Elohim.Models;

namespace Muebles_Elohim.Services
{
    // Lee el catálogo de Supabase y lo guarda en memoria unos minutos
    public class CatalogoServicio
    {
        private const string LlaveCache = "catalogo_fotos";
        private static readonly TimeSpan Duracion = TimeSpan.FromMinutes(10);

        // Si Supabase falla, se reintenta pasado este tiempo (sirviendo la última copia buena)
        private static readonly TimeSpan DuracionRespaldo = TimeSpan.FromMinutes(1);

        // Evita que varios visitantes consulten Supabase a la vez cuando el caché está vacío
        private static readonly SemaphoreSlim Candado = new SemaphoreSlim(1, 1);

        // Última lista que Supabase devolvió bien (no vence)
        private static List<CatalogoFoto>? _ultimaCopiaBuena;

        private readonly Supabase.Client _supabase;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CatalogoServicio> _logger;

        public CatalogoServicio(Supabase.Client supabase, IMemoryCache cache, ILogger<CatalogoServicio> logger)
        {
            _supabase = supabase;
            _cache = cache;
            _logger = logger;
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

                try
                {
                    var respuesta = await _supabase.From<CatalogoFoto>().Get();

                    List<CatalogoFoto> lista = respuesta.Models
                        .OrderBy(p => p.Id)
                        .ToList();

                    // Una lista vacía no se guarda: así un fallo no se queda pegado 10 minutos
                    if (lista.Count > 0)
                    {
                        _cache.Set(LlaveCache, lista, Duracion);
                        _ultimaCopiaBuena = lista;
                    }

                    return new List<CatalogoFoto>(lista);
                }
                catch (Exception ex) when (_ultimaCopiaBuena != null)
                {
                    // Supabase falló: se sirve la última copia buena y se reintenta en 1 minuto
                    _logger.LogWarning(ex, "Supabase no respondió; se usa la última copia guardada del catálogo");

                    _cache.Set(LlaveCache, _ultimaCopiaBuena, DuracionRespaldo);

                    return new List<CatalogoFoto>(_ultimaCopiaBuena);
                }
            }
            finally
            {
                Candado.Release();
            }
        }
    }
}