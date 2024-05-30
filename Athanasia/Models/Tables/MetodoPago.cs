using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Athanasia.Models.Tables
{
    [Table("METODO_PAGO")]
    public class MetodoPago
    {
        [Key]
        [Column("ID_METODO_PAGO")]
        public int IdMetodoPago { get; set; }
        [Column("NOMBRE")]
        public string? Nombre { get; set; }
    }
}
