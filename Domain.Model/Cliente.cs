using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Domain.Model
{
    public class Cliente 
    {
        public int id_cliente { get; set; } // Clave Primaria
        public string email { get; set; }
        [ValidateNever]
        public string password_hash { get; set; }
        public DateTime fecha_registro { get; set; }
        public string nombre { get; set; }
        public string ubicacion { get; set; }

        public ICollection<Receta> RecetasCreadas { get; set; } // Un Cliente crea muchas Recetas
        public ICollection<Favorito> Favoritos { get; set; } // Un Cliente tiene muchos Favoritos
        public ICollection<PedidoOferta> PedidosRealizados { get; set; } // Un Cliente realiza muchos Pedido_Ofertas 

        public Cliente()
        {
            RecetasCreadas = new HashSet<Receta>();
            Favoritos = new HashSet<Favorito>();
            PedidosRealizados = new HashSet<PedidoOferta>();
        }
    }
}