namespace Muebles_Elohim.Models
{
    // Un producto dentro de la lista de cotización (vive en el navegador del cliente)
    public class ItemCotizacion
    {
        public string ProductoId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
    }
}