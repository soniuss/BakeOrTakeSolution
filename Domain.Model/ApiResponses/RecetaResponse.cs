using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.ApiResponses
{
    public class RecetaResponse
    {
        // Propiedades de la Receta (PascalCase para DTOs)
        public int IdReceta { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ingredientes { get; set; }
        public string Pasos { get; set; }
        public string ImagenUrl { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedades del Cliente Creador (aplanadas para evitar ciclos)
        public int IdClienteCreador { get; set; }
        public string ClienteCreadorNombre { get; set; }
        public string ClienteCreadorEmail { get; set; }
        // No incluyas colecciones de navegación (RecetasCreadas, PedidosYOfertas, Favoritos)
        // ya que eso volvería a introducir los ciclos y la complejidad en la serialización.
    }
}
