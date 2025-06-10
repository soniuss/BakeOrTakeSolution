using System.ComponentModel.DataAnnotations;

namespace Domain.Model.ApiRequests
{
    public class OfertaRequest
    {
        [Required]
        public double Precio { get; set; } // Precio de la oferta

        [Required]
        public bool Disponibilidad { get; set; } // Disponibilidad de la oferta

        [MaxLength(500)]
        public string DescripcionOferta { get; set; } // Descripción específica de la oferta

        // El id_receta se pasará en la URL o en el cuerpo.
        // El id_empresa se obtendrá del token JWT.
    }
}
