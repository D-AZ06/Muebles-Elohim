using Microsoft.AspNetCore.Components;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;
using Microsoft.JSInterop;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Cotizacion : IDisposable
    {
        [Inject]
        private ListaCotizacionServicio Lista { get; set; } = default!;

        [Inject]
        private CotizacionServicio Cotizaciones { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        private ItemCotizacion? ultimoQuitado;

        private SolicitudCotizacion _solicitud = new SolicitudCotizacion();

        protected override void OnInitialized()
        {
            Lista.OnChange += Refrescar;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Lista.InicializarAsync();
            }
        }

        private void Refrescar()
        {
            InvokeAsync(StateHasChanged);
        }

        private async Task CambiarCantidad(ItemCotizacion item, int cantidad)
        {
            await Lista.CambiarCantidadAsync(item.ProductoId, cantidad);
        }

        private async Task Quitar(ItemCotizacion item)
        {
            ultimoQuitado = new ItemCotizacion
            {
                ProductoId = item.ProductoId,
                Nombre = item.Nombre,
                Imagen = item.Imagen,
                Cantidad = item.Cantidad
            };
            await Lista.QuitarAsync(item.ProductoId);
        }

        private async Task Deshacer()
        {
            if (ultimoQuitado is null) return;

            await Lista.AgregarAsync(ultimoQuitado.ProductoId, ultimoQuitado.Nombre, ultimoQuitado.Imagen);
            await Lista.CambiarCantidadAsync(ultimoQuitado.ProductoId, ultimoQuitado.Cantidad);
            ultimoQuitado = null;
        }

        private async Task Vaciar()
        {
            if (await JS.InvokeAsync<bool>("confirm", "¿Vaciar toda la lista?"))
            {
                ultimoQuitado = null;
                await Lista.VaciarAsync();
            }
        }

        // El botón de WhatsApp solo se activa con nombre y teléfono razonables
        private bool DatosValidos()
        {
            if (_solicitud.Nombre.Trim().Length < 2) return false;
            if (!_solicitud.AceptaDatos) return false;

            int digitos = 0;
            foreach (char c in _solicitud.Telefono)
            {
                if (char.IsDigit(c)) digitos++;
            }

            return digitos >= 7;
        }

        private string ObtenerUrlWhatsApp()
        {
            return Cotizaciones.ConstruirUrlWhatsApp(_solicitud, Lista.Items);
        }

        public void Dispose()
        {
            Lista.OnChange -= Refrescar;
        }
    }
}