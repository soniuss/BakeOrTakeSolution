using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Empresa 
    {
        public string id_empresa { get; set; } // Clave Primaria

        public string email { get; set; } // Datos de Usuario movidos aquí
        public string password { get; set; } // Considera hashing
        

        public string nombreNegocio { get; set; }
        public string descripcion { get; set; }
        public string ubicacion { get; set; }

        // Propiedad de navegación
        public ICollection<PedidoOferta> OfertasYPedidos { get; set; } // Una Empresa crea/ofrece muchos Pedido_Ofertas

        public Empresa()
        {
            OfertasYPedidos = new HashSet<PedidoOferta>();
        }
    }
}