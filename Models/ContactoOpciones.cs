namespace Muebles_Elohim.Models
{
    // Datos de contacto del taller. Los valores NO van en el código:
    // vienen de secrets.json (local) o de variables de entorno (producción)
    public class ContactoOpciones
    {
        public const string Seccion = "Contacto";

        public string Telefono { get; set; } = string.Empty;

        // Formato internacional sin "+", por ejemplo 57300XXXXXXX
        public string WhatsApp { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
    }
}