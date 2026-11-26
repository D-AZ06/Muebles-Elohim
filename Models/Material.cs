using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Muebles_Elohim.Models
{
    [Table("materiales")]
    public class Material : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Column("foto")]
        public string Foto { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}