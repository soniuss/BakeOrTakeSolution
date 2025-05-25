using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Empresa 
    {
        public int id_empresa { get; set; } // Clave Primaria
        public DateTime fecha_registro { get; set; }
        public string email { get; set; } 
        public string password_hash { get; set; }
        public string nombre_negocio { get; set; }
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