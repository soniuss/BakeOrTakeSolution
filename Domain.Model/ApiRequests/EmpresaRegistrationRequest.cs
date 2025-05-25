using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.ApiRequests
{
    public class EmpresaRegistrationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string NombreNegocio { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
    }
}
