using Microsoft.Extensions.Caching.Memory;
using Muebles_Elohim.Models;

namespace Muebles_Elohim.Services
{
    public class MaterialesServicio
    {
        private const string LlaveCache = "materiales";
        private readonly Supabase.Client _supabase;
        private readonly IMemoryCache _cache;

        public MaterialesServicio(Supabase.Client supabase, IMemoryCache cache)
        {
            _supabase = supabase;
            _cache = cache;
        }

        public async Task<List<Material>> ObtenerAsync()
        {
            if (_cache.TryGetValue(LlaveCache, out List<Material>? guardados) && guardados != null)
            {
                return new List<Material>(guardados);
            }

            var respuesta = await _supabase.From<Material>().Get();
            List<Material> lista = respuesta.Models.OrderBy(m => m.Id).ToList();

            if (lista.Count > 0)
            {
                _cache.Set(LlaveCache, lista, TimeSpan.FromMinutes(10));
            }

            return lista;
        }
    }
}