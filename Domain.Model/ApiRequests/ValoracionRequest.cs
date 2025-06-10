using System.ComponentModel.DataAnnotations;

namespace Domain.Model.ApiRequests
{
    public class ValoracionRequest
    {
        [Required]
        [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5.")]
        public int Puntuacion { get; set; }

        [MaxLength(500)]
        public string Comentario { get; set; }
    }
}
