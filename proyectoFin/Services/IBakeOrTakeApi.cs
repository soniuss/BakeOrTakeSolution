using Domain.Model; // Necesario para Receta (en los cuerpos de POST/PUT) y otras entidades
using Domain.Model.ApiRequests; // ¡IMPORTANTE! Para RecetaResponse
using Domain.Model.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace proyectoFin.Services
{
    public interface IBakeOrTakeApi
    {
        // Métodos de autenticación (estos no cambian)
        [Post("/api/Auth/register/cliente")]
        Task<ApiResponse<Domain.Model.Cliente>> RegisterClienteAsync([Body] ClienteRegistrationRequest request);

        [Post("/api/Auth/register/empresa")]
        Task<ApiResponse<Domain.Model.Empresa>> RegisterEmpresaAsync([Body] EmpresaRegistrationRequest request);

        [Post("/api/Auth/login")]
        Task<ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);

        // --- Metodos para Clientes ----
        [Get("/api/Clientes/{id}")]
        Task<ApiResponse<Cliente>> GetClienteByIdAsync(int id);
        [Get("/api/Favoritos/ByClient/{id_cliente}")]
        Task<ApiResponse<List<RecetaResponse>>> GetFavoritosByClientAsync(int id_cliente);
        // --- Métodos para Recetas ---

        // Obtener todas las recetas
        [Get("/api/Recetas")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasAsync(); // ¡CAMBIO AQUÍ!

        // Obtener una receta por ID
        [Get("/api/Recetas/{id}")]
        Task<ApiResponse<RecetaResponse>> GetRecetaByIdAsync(int id); // ¡CAMBIO AQUÍ!

        // Crear una nueva receta (envía Receta, recibe RecetaResponse)
        [Post("/api/Recetas")]
        Task<ApiResponse<RecetaResponse>> CreateRecetaAsync([Body] Receta newReceta); // ¡CAMBIO AQUÍ! (Retorno)

        // Actualizar una receta existente (envía Receta, recibe RecetaResponse)
        [Put("/api/Recetas/{id}")]
        Task<ApiResponse<RecetaResponse>> UpdateRecetaAsync(int id, [Body] Receta updatedReceta); // ¡CAMBIO AQUÍ! (Retorno)

        // Obtener recetas creadas por un cliente específico
        [Get("/api/Recetas/ByCreator/{id_cliente_creador}")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasByCreatorAsync(int id_cliente_creador); // ¡CAMBIO AQUÍ!

        // Eliminar una receta (no devuelve contenido específico, por eso object)
        [Delete("/api/Recetas/{id}")]
        Task<ApiResponse<object>> DeleteReceta(int id);

        // Obtener recetas gestionadas/creadas por una empresa específica
        [Get("/api/Recetas/ByCompany/{id_empresa}")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasByCompanyAsync(int id_empresa); // ¡CAMBIO AQUÍ!

        // --- Métodos para Empresa (Perfil) ---
        // Obtener perfil de empresa por ID (si devuelve la entidad completa Empresa, está bien)
        [Get("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> GetEmpresaByIdAsync(int id);

        // Actualizar perfil de empresa por ID (si recibe y devuelve la entidad completa Empresa, está bien)
        [Put("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> UpdateEmpresaAsync(int id, [Body] Empresa updateData);

        // --- Métodos para Pedidos/Ofertas ---

        // Obtener ofertas para una receta específica
        [Get("/api/PedidosOfertas/ByReceta/{id_receta}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetOffersByRecetaAsync(int id_receta);

        // Crear una oferta (por una Empresa)
        [Post("/api/PedidosOfertas/offer/{id_receta}")]
        Task<ApiResponse<PedidoOfertaResponse>> CreateOfferAsync(int id_receta, [Body] OfertaRequest request);

        // Realizar un pedido (por un Cliente)
        [Post("/api/PedidosOfertas/order")]
        Task<ApiResponse<PedidoOfertaResponse>> PlaceOrderAsync([Body] PedidoRequest request);

        // Obtener pedidos realizados por un cliente
        [Get("/api/PedidosOfertas/ByClient/{id_cliente}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetClientOrdersAsync(int id_cliente); // ¡CAMBIO AQUÍ!

        // Obtener ofertas y pedidos de una empresa
        [Get("/api/PedidosOfertas/ByCompany/{id_empresa}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetCompanyOffersAsync(int id_empresa); // ¡CAMBIO AQUÍ! (Y añadir si faltaba)

        // NUEVO: Marcar un pedido como completado (por Empresa)
        [HttpPut("/api/PedidosOfertas/complete/{id_pedido_oferta}")]
        Task<ApiResponse<object>> CompleteOrderAsync(int id_pedido_oferta); // Devuelve 204 No Content

        // NUEVO: Valorar un pedido completado (por Cliente)
        [HttpPut("/api/PedidosOfertas/rate/{id_pedido_oferta}")]
        Task<ApiResponse<object>> RateOrderAsync(int id_pedido_oferta, [Body] ValoracionRequest request); // Devuelve 204 No Content
        
        // NUEVO: Eliminar una oferta (por Empresa)
        [HttpDelete("/api/PedidosOfertas/{id_pedido_oferta}")]
        Task<ApiResponse<object>> DeleteOfferAsync(int id_pedido_oferta); // Devuelve 204 No Content

    }

}

