namespace Muebles_Elohim.Models
{
    // Datos que escribe el cliente antes de abrir WhatsApp (no se guardan en ningún lado)
    public class SolicitudCotizacion
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }
}