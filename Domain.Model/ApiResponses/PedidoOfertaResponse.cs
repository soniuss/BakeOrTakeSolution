// En Domain.Model/ApiResponses/PedidoOfertaResponse.cs
using System;

namespace Domain.Model.ApiResponses
{
    public class PedidoOfertaResponse
    {
        public int IdPedidoOferta { get; set; }
        public double Precio { get; set; }
        public bool Disponibilidad { get; set; }
        public string DescripcionOferta { get; set; }

        // Información de la Empresa que ofrece
        public int IdEmpresa { get; set; }
        public string EmpresaNombreNegocio { get; set; }
        public string EmpresaUbicacion { get; set; }

        // Información de la Receta a la que se refiere
        public int IdReceta { get; set; }
        public string RecetaNombre { get; set; }
        public string RecetaImagenUrl { get; set; }

        // Información del Cliente que realiza el Pedido (será null si es solo una oferta)
        public int? IdClienteRealiza { get; set; }
        public string ClienteRealizaNombre { get; set; }
        public string ClienteRealizaEmail { get; set; }

        // Datos del Pedido
        public DateTime? FechaPedido { get; set; }
        public string Estado { get; set; } // Ej: "Oferta", "Pedido_Pendiente", "Pedido_Completado"

        // Datos de la Valoración (si existe)
        public int? Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime? FechaValoracion { get; set; }
    }
}
