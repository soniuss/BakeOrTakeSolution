
namespace Domain.Model.ApiRequests
{
    public class EmpresaRegistrationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string NombreNegocio { get; set; } // Propiedad clara para el nombre del negocio (usada en el DTO)
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
     
    }
}