using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.ApiRequests
{
    public class LoginResponse
    {
        public string Token { get; set; } // Token JWT para futuras peticiones autenticadas
        public string UserType { get; set; } // "Cliente" o "Empresa"
        public int UserId { get; set; } // ID del usuario autenticado
        // Puedes añadir mas propiedades segun lo que tu API devuelva (ej. Nombre de usuario)
    }
}
