using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Muebles_Elohim.Models
{
    [Table("catalogo_fotos")]
    public class CatalogoFoto : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("foto")]
        public string Foto { get; set; } = string.Empty;

        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
