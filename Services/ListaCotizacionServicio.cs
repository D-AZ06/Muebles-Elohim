using Microsoft.JSInterop;
using Muebles_Elohim.Models;
using System.Text.Json;
using System.Xml.Linq;

namespace Muebles_Elohim.Services
{
    // Guarda la lista de cotización en memoria y en localStorage del navegador
    public class ListaCotizacionServicio
    {
        private const string LlaveStorage = "muebles_elohim_cotizacion";
        private const int CantidadMaxima = 99;

        private readonly IJSRuntime _js;
        private readonly List<ItemCotizacion> _items = new List<ItemCotizacion>();
        private bool _cargando = false;
        private bool _cargado = false;

        // Los componentes se suscriben para redibujarse cuando la lista cambia
        public event Action? OnChange;

        public IReadOnlyList<ItemCotizacion> Items
        {
            get { return _items; }
        }

        public ListaCotizacionServicio(IJSRuntime js)
        {
            _js = js;
        }

        public int TotalUnidades()
        {
            int total;
            total = 0;

            foreach (ItemCotizacion item in _items)
            {
                total = total + item.Cantidad;
            }

            return total;
        }

        public bool Contiene(string productoId)
        {
            foreach (ItemCotizacion item in _items)
            {
                if (item.ProductoId == productoId)
                {
                    return true;
                }
            }

            return false;
        }

        public int CantidadDe(string productoId)
        {
            foreach (ItemCotizacion item in _items)
            {
                if (item.ProductoId == productoId)
                {
                    return item.Cantidad;
                }
            }

            return 0;
        }

        // Se llama desde OnAfterRenderAsync (antes no hay JavaScript disponible)
        public async Task InicializarAsync()
        {
            if (_cargado || _cargando)
            {
                return;
            }

            _cargando = true;

            try
            {
                string? json;
                json = await _js.InvokeAsync<string?>("localStorage.getItem", LlaveStorage);

                if (!string.IsNullOrEmpty(json))
                {
                    List<ItemCotizacion>? guardados;
                    guardados = JsonSerializer.Deserialize<List<ItemCotizacion>>(json);

                    if (guardados != null)
                    {
                        foreach (ItemCotizacion guardado in guardados)
                        {
                            // No se duplica lo que ya se agregó mientras cargaba
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
                // Datos dañados en el navegador: se ignoran y se empieza de cero
                _cargado = true;
            }
            catch (InvalidOperationException)
            {
                // Todavía sin JavaScript (prerender): se reintenta en el siguiente render
            }
            catch (JSDisconnectedException)
            {
                // El circuito se cerró: no hay nada que hacer
            }
            finally
            {
                _cargando = false;
            }

            OnChange?.Invoke();
        }

        public async Task AgregarAsync(string productoId, string nombre, string imagen)
        {
            ItemCotizacion? existente;
            existente = null;

            foreach (ItemCotizacion item in _items)
            {
                if (item.ProductoId == productoId)
                {
                    existente = item;
                }
            }

            if (existente != null)
            {
                if (existente.Cantidad < CantidadMaxima)
                {
                    existente.Cantidad = existente.Cantidad + 1;
                }
            }
            else
            {
                ItemCotizacion nuevo = new ItemCotizacion();
                nuevo.ProductoId = productoId;
                nuevo.Nombre = nombre;
                nuevo.Imagen = imagen;
                nuevo.Cantidad = 1;
                _items.Add(nuevo);
            }

            await GuardarAsync();
        }

        public async Task CambiarCantidadAsync(string productoId, int cantidad)
        {
            if (cantidad <= 0)
            {
                await QuitarAsync(productoId);
                return;
            }

            if (cantidad > CantidadMaxima)
            {
                cantidad = CantidadMaxima;
            }

            foreach (ItemCotizacion item in _items)
            {
                if (item.ProductoId == productoId)
                {
                    item.Cantidad = cantidad;
                }
            }

            await GuardarAsync();
        }

        public async Task QuitarAsync(string productoId)
        {
            ItemCotizacion? aQuitar;
            aQuitar = null;

            foreach (ItemCotizacion item in _items)
            {
                if (item.ProductoId == productoId)
                {
                    aQuitar = item;
                }
            }

            if (aQuitar != null)
            {
                _items.Remove(aQuitar);
            }

            await GuardarAsync();
        }

        public async Task VaciarAsync()
        {
            _items.Clear();
            await GuardarAsync();
        }

        private async Task GuardarAsync()
        {
            try
            {
                string json;
                json = JsonSerializer.Serialize(_items);
                await _js.InvokeVoidAsync("localStorage.setItem", LlaveStorage, json);
            }
            catch (InvalidOperationException)
            {
                // Sin JavaScript disponible: la lista queda solo en memoria
            }
            catch (JSDisconnectedException)
            {
                // El circuito se cerró
            }

            OnChange?.Invoke();
        }
    }
}