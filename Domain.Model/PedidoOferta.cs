using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class PedidoOferta // Representa tanto una Oferta como un Pedido
    {
        public string id_pedido_oferta { get; set; } // Clave Primaria

        // --- Datos de la Oferta ---
        public double precio { get; set; }
        public bool disponibilidad { get; set; } // O el tipo adecuado
        public string descripcionOferta { get; set; } // Descripción específica de esta oferta/pedido

        // Enlaces a Empresa y Receta (lado 'muchos' en las relaciones 1:N)
        public string id_empresa { get; set; } // Clave Foránea (La empresa que la ofrece/crea)
        public Empresa Empresa { get; set; } // Propiedad de navegación a la Empresa

        public string id_receta { get; set; } // Clave Foránea (La receta a la que se refiere la oferta/pedido)
        public Receta Receta { get; set; } // Propiedad de navegación a la Receta

        // --- Datos del Pedido (SI tiene Cliente) ---
        // Este id_cliente es OPCIONAL (nullable)
        public string? id_cliente { get; set; } // Clave Foránea OPCIONAL (será NULL si es solo una oferta)
        public Cliente ClienteRealiza { get; set; } // Propiedad de navegación al Cliente (será NULL si es solo una oferta)

        public DateTime? fechaPedido { get; set; } // Será NULL si es solo una oferta
        public string estado { get; set; } // Ej: "Oferta", "Pedido_Pendiente", "Pedido_Completado"

        // --- Datos de la Valoración (EMBEBIDA, SI es un Pedido y está valorado) ---
        public int? puntuacion { get; set; } // Será NULL si no está valorado o no es un pedido
        public string? comentario { get; set; } // Será NULL si no está valorado o no es un pedido
        public DateTime? fechaValoracion { get; set; } // Será NULL si no está valorado o no es un pedido


        public PedidoOferta()
        {
            // Puedes generar el ID aquí
        }
    }
}