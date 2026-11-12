using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Muebles_Elohim.Models;


namespace Muebles_Elohim.Components.Pages
{
    public partial class Contacto : ComponentBase
    {
        [Inject]
        private IOptions<ContactoOpciones> Opciones { get; set; } = default!;

        private string urlWhatsApp = string.Empty;
        private string mensajePredeterminado = Uri.EscapeDataString("¡Hola! Me comunico desde la página web de Muebles Elohim, me gustaría solicitar información y cotización sobre sus productos.");

        protected override void OnInitialized()
        {
            // Obtiene el número directamente desde la sección "Contacto" de tu configuración
            var numeroTelefono = Opciones.Value.WhatsApp;

            // Construye la URL de manera dinámica
            urlWhatsApp = $"https://wa.me/{numeroTelefono}?text={mensajePredeterminado}";
        }
    }
}