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

        // Si el enlace pasa de este largo, algunos navegadores lo cortan
        private const int LargoMaximoUrl = 4000;

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

            string url = ArmarUrl(numero, solicitud, items, true);

            // Con muchos productos se quitan los enlaces para que el mensaje no se corte
            if (url.Length > LargoMaximoUrl)
            {
                url = ArmarUrl(numero, solicitud, items, false);
            }

            return url;
        }

        private string ArmarUrl(string numero, SolicitudCotizacion solicitud, IReadOnlyList<ItemCotizacion> items, bool conEnlaces)
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("Hola, soy " + solicitud.Nombre.Trim() + " (" + solicitud.Telefono.Trim() + ").");
            texto.AppendLine("Quiero cotizar:");
            texto.AppendLine();

            int numeroItem = 1;

            foreach (ItemCotizacion item in items)
            {
                // Tipo de mueble + modelo (si el nombre ya empieza con el tipo, no se repite)
                string producto = item.Nombre;

                if (!string.IsNullOrWhiteSpace(item.Categoria) &&
                    !item.Nombre.StartsWith(item.Categoria, StringComparison.OrdinalIgnoreCase))
                {
                    producto = item.Categoria + " - " + item.Nombre;
                }

                texto.AppendLine(numeroItem + ". " + producto + " (x" + item.Cantidad + ")");

                if (conEnlaces)
                {
                    texto.AppendLine(EnlaceProducto(item.ProductoId));
                }
                else
                {
                    texto.AppendLine("ref. " + item.ProductoId);
                }

                texto.AppendLine();
                numeroItem++;
            }

            return "https://wa.me/" + numero + "?text=" + Uri.EscapeDataString(texto.ToString().TrimEnd());
        }

        private string EnlaceProducto(string id)
        {
            string ruta = id.StartsWith("mat-")
                ? "materiales?id=" + Uri.EscapeDataString(id.Substring(4))
                : "productos?id=" + Uri.EscapeDataString(id);  

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