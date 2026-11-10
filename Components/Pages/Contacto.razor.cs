using Microsoft.AspNetCore.Components;


namespace Muebles_Elohim.Components.Pages
{
    public partial class Contacto : ComponentBase
    {
        private string numeroTelefono = "573175223192";
        private string mensajePredeterminado = Uri.EscapeDataString("¡Hola! Me comunico desde la página web de Muebles Elohim, me gustaría solicitar información y cotización sobre sus productos.");
        private string urlWhatsApp = "https://wa.me/573175223192?text=¡Hola! Me comunico desde la página web de Muebles Elohim, me gustaría solicitar información y cotización sobre sus productos.";

        protected override void OnInitialized()
        {
            urlWhatsApp = $"https://wa.me/{numeroTelefono}?text={mensajePredeterminado}";
        }
    }
}