using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Muebles_Elohim.Models;

namespace Muebles_Elohim.Components.Pages
{
    public partial class Privacidad : ComponentBase
    {
        [Inject]
        private IOptions<ContactoOpciones> Opciones { get; set; } = default!;

        protected string correoContacto = string.Empty;
        protected string fechaActualizacion = "Noviembre de 2026";

        protected override void OnInitialized()
        {
            // Obtiene el correo directamente desde la sección "Contacto" de tu configuración
            var _correoContacto = Opciones.Value.Correo;
            correoContacto = _correoContacto;
        }
        
    }
}