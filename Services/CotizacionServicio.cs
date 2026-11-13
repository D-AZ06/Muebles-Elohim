using System.Text;
using Microsoft.Extensions.Options;
using Muebles_Elohim.Models;

namespace Muebles_Elohim.Services
{
    // Arma el enlace de WhatsApp con el mensaje de cotización. No guarda nada.
    public class CotizacionServicio
    {
        private readonly ContactoOpciones _contacto;

        public CotizacionServicio(IOptions<ContactoOpciones > contacto)
        {
            _contacto = contacto.Value;
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
            string numero;
            numero = SoloDigitos(_contacto.WhatsApp);

            // Formato fijo del mensaje: así un bot lo podrá leer fácilmente más adelante
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Hola, soy " + solicitud.Nombre.Trim() + " (" + solicitud.Telefono.Trim() + ").");
            texto.AppendLine("Quiero cotizar:");

            foreach (ItemCotizacion item in items)
            {
                texto.AppendLine("- " + item.Nombre + " x" + item.Cantidad);
            }

            string codificado;
            codificado = Uri.EscapeDataString(texto.ToString());

            return "https://wa.me/" + numero + "?text=" + codificado;
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