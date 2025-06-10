using System.ComponentModel.DataAnnotations;

namespace Domain.Model.ApiRequests
{
    public class PedidoRequest
    {
        [Required]
        public int IdPedidoOferta { get; set; } // El ID de la oferta a la que se hace el pedido

        // El id_cliente se obtendrá del token JWT.
        // La fechaPedido y el estado se asignan en el controlador.
    }
}
