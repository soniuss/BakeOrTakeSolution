using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Receta
    {
        public int id_receta { get; set; } // Clave Primaria
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string ingredientes { get; set; }
        public string pasos { get; set; }
        public string imagenUrl { get; set; }
        public DateTime fecha_registro { get; set; }

        // Enlace al Cliente creador (lado 'muchos' en la relación 1:N Cliente crea Receta)
        public int id_cliente_creador { get; set; } // Clave Foránea
        public Cliente ClienteCreador { get; set; } // Propiedad de navegación al Cliente

        // Relaciones con otras entidades (ahora Pedido_Oferta y Favorito)
        public ICollection<PedidoOferta> PedidosYOfertas { get; set; } // Una Receta es base de muchos Pedido_Ofertas
        public ICollection<Favorito> Favoritos { get; set; } // Una Receta está en muchos registros de Favorito (M:N con Cliente)


        public Receta()
        {
            PedidosYOfertas = new HashSet<PedidoOferta>();
            Favoritos = new HashSet<Favorito>();
        }
    }
}
