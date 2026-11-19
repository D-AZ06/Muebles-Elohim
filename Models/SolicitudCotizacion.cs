namespace Muebles_Elohim.Models
{
    // Datos que escribe el cliente antes de abrir WhatsApp (no se guardan en ningún lado)
    public class SolicitudCotizacion
    {
        public string Codigo { get; } = "COT-" + Guid.NewGuid().ToString("N")[..5].ToUpperInvariant();
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool AceptaDatos { get; set; }

    }
}