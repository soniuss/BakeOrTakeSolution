// En Domain.Model/ApiRequests/FavoritoToggleRequest.cs (crea este archivo)
using System.ComponentModel.DataAnnotations;

namespace Domain.Model.ApiRequests
{
    public class FavoritoToggleRequest
    {
        [Required]
        public int IdReceta { get; set; }
        // El IdCliente se obtendrá del token JWT en la API
    }
}
