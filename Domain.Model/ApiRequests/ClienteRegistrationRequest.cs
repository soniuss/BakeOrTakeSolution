using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.ApiRequests
{
    public class ClienteRegistrationRequest
    {
        public string Email { get; set; }   
        public string Password { get; set; } 
        public string Nombre { get; set; }    
        public string Ubicacion { get; set; } 
    }
}