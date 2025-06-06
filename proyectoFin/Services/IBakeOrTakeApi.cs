using Domain.Model; // Necesario para Cliente y Empresa (en retornos de Auth y Perfil) y Receta (en cuerpos de POST/PUT)
using Domain.Model.ApiRequests; // Para DTOs de solicitud como ClienteRegistrationRequest, OfertaRequest, PedidoRequest, ValoracionRequest
using Domain.Model.ApiResponses; // ¡IMPORTANTE! Para DTOs de respuesta como RecetaResponse, PedidoOfertaResponse, LoginResponse
using Microsoft.AspNetCore.Mvc;
using Refit; // Para atributos HTTP y ApiResponse

namespace proyectoFin.Services
{
    public interface IBakeOrTakeApi
    {
        // --- Métodos de Autenticación ---
        [Post("/api/Auth/register/cliente")]
        Task<ApiResponse<Domain.Model.Cliente>> RegisterClienteAsync([Body] ClienteRegistrationRequest request);

        [Post("/api/Auth/register/empresa")]
        Task<ApiResponse<Domain.Model.Empresa>> RegisterEmpresaAsync([Body] EmpresaRegistrationRequest request);

        [Post("/api/Auth/login")]
        Task<ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);

        // --- Métodos para Recetas ---
        [Get("/api/Recetas")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasAsync();

        [Get("/api/Recetas/{id}")]
        Task<ApiResponse<RecetaResponse>> GetRecetaByIdAsync(int id);

        [Post("/api/Recetas")]
        Task<ApiResponse<RecetaResponse>> CreateRecetaAsync([Body] Receta newReceta);

        [Put("/api/Recetas/{id}")]
        Task<ApiResponse<RecetaResponse>> UpdateRecetaAsync(int id, [Body] Receta updatedReceta);

        [Get("/api/Recetas/ByCreator/{id_cliente_creador}")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasByCreatorAsync(int id_cliente_creador);

        [Delete("/api/Recetas/{id}")]
        Task<ApiResponse<object>> DeleteReceta(int id);

        [Get("/api/Recetas/ByCompany/{id_empresa}")]
        Task<ApiResponse<List<RecetaResponse>>> GetRecetasByCompanyAsync(int id_empresa);

        // --- Métodos para Cliente (Perfil) ---
        [Get("/api/Clientes/{id}")]
        Task<ApiResponse<Cliente>> GetClienteByIdAsync(int id);

       //Actualizar perfil de cliente por ID
        [Put("/api/Clientes/{id}")]
        Task<ApiResponse<object>> UpdateClienteAsync(int id, [Body] Cliente updateData);

        // --- Métodos para Empresa (Perfil) ---
        // Este ya debería estar, pero lo reconfirmo.
        [Get("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> GetEmpresaByIdAsync(int id);

        // Este ya debería estar, pero lo reconfirmo.
        [Put("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> UpdateEmpresaAsync(int id, [Body] Empresa updateData);

        // --- Métodos para Pedidos/Ofertas ---
        [Get("/api/PedidosOfertas/ByReceta/{id_receta}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetOffersByRecetaAsync(int id_receta);

        [Post("/api/PedidosOfertas/offer/{id_receta}")]
        Task<ApiResponse<PedidoOfertaResponse>> CreateOfferAsync(int id_receta, [Body] OfertaRequest request);

        [Post("/api/PedidosOfertas/order")]
        Task<ApiResponse<PedidoOfertaResponse>> PlaceOrderAsync([Body] PedidoRequest request);

        // Este ya debería estar, pero lo reconfirmo.
        [Get("/api/PedidosOfertas/ByClient/{id_cliente}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetClientOrdersAsync(int id_cliente);

        // Este ya debería estar, pero lo reconfirmo.
        [Get("/api/PedidosOfertas/ByCompany/{id_empresa}")]
        Task<ApiResponse<List<PedidoOfertaResponse>>> GetCompanyOffersAsync(int id_empresa);

        [Put("/api/PedidosOfertas/complete/{id_pedido_oferta}")]
        Task<ApiResponse<object>> CompleteOrderAsync(int id_pedido_oferta);

        [Put("/api/PedidosOfertas/rate/{id_pedido_oferta}")]
        Task<ApiResponse<object>> RateOrderAsync(int id_pedido_oferta, [Body] ValoracionRequest request);

        [Get("/api/Favoritos/ByClient/{id_cliente}")]
        Task<ApiResponse<List<RecetaResponse>>> GetFavoritosByClientAsync(int id_cliente);

        [Delete("/api/PedidosOfertas/{id_pedido_oferta}")]
        Task<ApiResponse<object>> DeleteOfferAsync(int id_pedido_oferta);

    }
}
