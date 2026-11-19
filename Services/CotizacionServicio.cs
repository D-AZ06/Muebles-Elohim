using System.Text;
using Microsoft.Extensions.Options;
using Muebles_Elohim.Models;
using Microsoft.AspNetCore.Components;

namespace Muebles_Elohim.Services
{
    // Arma el enlace de WhatsApp con el mensaje de cotización. No guarda nada.
    public class CotizacionServicio
    {
        private readonly ContactoOpciones _contacto;
        private readonly NavigationManager _navegacion;

        public CotizacionServicio(IOptions<ContactoOpciones> contacto, NavigationManager navegacion)
        {
            _contacto = contacto.Value;
            _navegacion = navegacion;
        }


        public bool TieneNumeroConfigurado()
        {
            string numero;
            numero = SoloDigitos(_contacto.WhatsApp);

            if (numero.Length == 0)
            {
                return false;
            }

            return true;
        }

        public string ConstruirUrlWhatsApp(SolicitudCotizacion solicitud, IReadOnlyList<ItemCotizacion> items)
        {
            string numero = SoloDigitos(_contacto.WhatsApp);

            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Cotización " + solicitud.Codigo);
            texto.AppendLine("Hola, soy " + solicitud.Nombre.Trim() + " (" + solicitud.Telefono.Trim() + ").");
            texto.AppendLine("Quiero cotizar:");

            foreach (ItemCotizacion item in items)
            {
                texto.AppendLine("- " + item.Nombre + " x" + item.Cantidad + " | " + EnlaceProducto(item.ProductoId));
            }

            return "https://wa.me/" + numero + "?text=" + Uri.EscapeDataString(texto.ToString());
        }

        private string EnlaceProducto(string id)
        {
            string ruta = "productos?id=" + Uri.EscapeDataString(id);

            if (!string.IsNullOrWhiteSpace(_contacto.SitioUrl))
            {
                return _contacto.SitioUrl.TrimEnd('/') + "/" + ruta;
            }

            return _navegacion.ToAbsoluteUri(ruta).ToString();
        }

        // wa.me exige solo números: sin "+", espacios ni guiones
        private string SoloDigitos(string valor)
        {
            StringBuilder resultado = new StringBuilder();

            foreach (char caracter in valor)
            {
                if (char.IsDigit(caracter))
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString();
        }
    }
}