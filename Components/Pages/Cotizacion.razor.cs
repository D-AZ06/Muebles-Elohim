using Microsoft.AspNetCore.Components;
using Muebles_Elohim.Models;
using Muebles_Elohim.Services;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Cotizacion : IDisposable
    {
        [Inject]
        private ListaCotizacionServicio Lista { get; set; } = default!;

        [Inject]
        private CotizacionServicio Cotizaciones { get; set; } = default!;

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
            await Lista.QuitarAsync(item.ProductoId);
        }

        // El botón de WhatsApp solo se activa con nombre y teléfono razonables
        private bool DatosValidos()
        {
            string nombre;
            nombre = _solicitud.Nombre.Trim();

            if (nombre.Length < 2)
            {
                return false;
            }

            int digitos;
            digitos = 0;

            foreach (char caracter in _solicitud.Telefono)
            {
                if (char.IsDigit(caracter))
                {
                    digitos = digitos + 1;
                }
            }

            if (digitos < 7)
            {
                return false;
            }

            return true;
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