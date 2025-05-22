using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Cliente 
    {
        public string id_cliente { get; set; } // Clave Primaria

        public string email { get; set; } // Los datos de Usuario se mueven aquí o se gestionan diferente
        public string password { get; set; } // Considera hashing
       
        public string nombre { get; set; }
        public string ubicacion { get; set; }

        // Propiedades de navegación
        public ICollection<Receta> RecetasCreadas { get; set; } // Un Cliente crea muchas Recetas
        public ICollection<Favorito> Favoritos { get; set; } // Un Cliente tiene muchos Favoritos (M:N con Receta)
        public ICollection<PedidoOferta> PedidosRealizados { get; set; } // Un Cliente realiza muchos Pedido_Ofertas (solo los que son pedidos)

        public Cliente()
        {
            RecetasCreadas = new HashSet<Receta>();
            Favoritos = new HashSet<Favorito>();
            PedidosRealizados = new HashSet<PedidoOferta>();
        }
    }
}