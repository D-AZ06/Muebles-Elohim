using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using System.Text.Json;

namespace Muebles_Elohim.Services
{
    // Guarda los favoritos en memoria y en localStorage del navegador
    public class FavoritosServicio
    {
        private const string LlaveStorage = "muebles_elohim_favoritos";

        private readonly IJSRuntime _js;
        private readonly List<ItemFavorito> _items = new List<ItemFavorito>();
        private Task? _tareaCarga;
        private bool _cargado = false;

        public event Action? OnChange;

        public IReadOnlyList<ItemFavorito> Items => _items;

        public FavoritosServicio(IJSRuntime js)
        {
            _js = js;
        }

        public bool Contiene(string productoId)
        {
            return _items.Any(i => i.ProductoId == productoId);
        }

        public async Task InicializarAsync()
        {
            if (_cargado)
            {
                return;
            }

            if (_tareaCarga != null)
            {
                await _tareaCarga;
                return;
            }

            _tareaCarga = CargarDesdeStorageAsync();

            try
            {
                await _tareaCarga;
            }
            finally
            {
                _tareaCarga = null;
            }
        }

        private async Task CargarDesdeStorageAsync()
        {
            try
            {
                string? json = await _js.InvokeAsync<string?>("localStorage.getItem", LlaveStorage);

                if (!string.IsNullOrEmpty(json))
                {
                    List<ItemFavorito>? guardados = JsonSerializer.Deserialize<List<ItemFavorito>>(json);

                    if (guardados != null)
                    {
                        foreach (ItemFavorito guardado in guardados)
                        {
                            if (!Contiene(guardado.ProductoId))
                            {
                                _items.Add(guardado);
                            }
                        }
                    }
                }

                _cargado = true;
            }
            catch (JsonException)
            {
                _cargado = true;
            }
            catch (InvalidOperationException)
            {
                // Todavía sin JavaScript (prerender): se reintenta en el siguiente render
            }
            catch (JSDisconnectedException)
            {
            }

            OnChange?.Invoke();
        }

        // Agrega si no está, quita si ya está
        public async Task AlternarAsync(string productoId, string nombre, string imagen)
        {
            ItemFavorito? existente = _items.FirstOrDefault(i => i.ProductoId == productoId);

            if (existente != null)
            {
                _items.Remove(existente);
            }
            else
            {
                _items.Add(new ItemFavorito
                {
                    ProductoId = productoId,
                    Nombre = nombre,
                    Imagen = imagen
                });
            }

            await GuardarAsync();
        }

        public async Task QuitarAsync(string productoId)
        {
            _items.RemoveAll(i => i.ProductoId == productoId);
            await GuardarAsync();
        }

        // Quita los productos que ya no existen y actualiza nombre y foto. Devuelve cuántos quitó.
        public async Task<int> SincronizarConCatalogoAsync(IEnumerable<CatalogoFoto> catalogo)
        {
            Dictionary<string, CatalogoFoto> porId = catalogo
                .GroupBy(p => p.Id.ToString())
                .ToDictionary(g => g.Key, g => g.First());

            if (porId.Count == 0)
            {
                return 0;
            }

            int quitados = 0;
            bool hayCambios = false;

            foreach (ItemFavorito item in _items.ToList())
            {
                if (!porId.TryGetValue(item.ProductoId, out CatalogoFoto? actual))
                {
                    _items.Remove(item);
                    quitados++;
                    hayCambios = true;
                }
                else if (item.Nombre != actual.Titulo || item.Imagen != actual.Foto)
                {
                    item.Nombre = actual.Titulo;
                    item.Imagen = actual.Foto;
                    hayCambios = true;
                }
            }

            if (hayCambios)
            {
                await GuardarAsync();
            }

            return quitados;
        }

        private async Task GuardarAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_items);
                await _js.InvokeVoidAsync("localStorage.setItem", LlaveStorage, json);
            }
            catch (InvalidOperationException)
            {
            }
            catch (JSDisconnectedException)
            {
            }

            OnChange?.Invoke();
        }
    }
}