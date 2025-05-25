using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Favorito
    {
        // Clave Primaria Compuesta (Configurada en DbContext)
        public int id_cliente { get; set; } // Clave Foránea
        public int id_receta { get; set; } // Clave Foránea

        // Propiedades de navegación a las entidades relacionadas
        public Cliente Cliente { get; set; }
        public Receta Receta { get; set; }

        // No necesita PK propia si usas la compuesta
    }
}