using Domain.Model;
using Domain.Model.ApiRequests;
using Refit;
using System.Collections.Generic;

namespace proyectoFin.Services
{
    public interface IBakeOrTakeApi
    {
        [Get("/api/Recetas")]
        Task<ApiResponse<List<Receta>>> GetRecetasAsync();

        [Post("/api/Auth/register/cliente")]
        Task<Refit.ApiResponse<Domain.Model.Cliente>> RegisterClienteAsync([Body] ClienteRegistrationRequest request);

        [Post("/api/Auth/register/empresa")]
        Task<Refit.ApiResponse<Domain.Model.Empresa>> RegisterEmpresaAsync([Body] EmpresaRegistrationRequest request);

        [Post("/api/Auth/login")]
        Task<ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);

        // --- Métodos para Recetas ---
        [Get("/api/Recetas/{id}")]
        Task<ApiResponse<Receta>> GetRecetaByIdAsync(int id);

        [Post("/api/Recetas")]
        Task<ApiResponse<Receta>> CreateRecetaAsync([Body] Receta newReceta);

        [Put("/api/Recetas/{id}")]
        Task<ApiResponse<Receta>> UpdateRecetaAsync(int id, [Body] Receta updatedReceta);

        [Get("/api/Recetas/ByCreator/{id_cliente_creador}")]
        Task<ApiResponse<List<Receta>>> GetRecetasByCreatorAsync(int id_cliente_creador);

        [Delete("/api/Recetas/{id}")]
        Task<ApiResponse<object>> DeleteReceta(int id);

        [Get("/api/Recetas/ByCompany/{id_empresa}")]
        Task<ApiResponse<List<Receta>>> GetRecetasByCompanyAsync(int id_empresa);

        // --- Métodos para Empresa (Perfil) ---
        // NUEVO: Obtener perfil de empresa por ID
        [Get("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> GetEmpresaByIdAsync(int id);

        // NUEVO: Actualizar perfil de empresa por ID
        [Put("/api/Empresas/{id}")]
        Task<ApiResponse<Empresa>> UpdateEmpresaAsync(int id, [Body] Empresa updateData);

        // --- Métodos para Pedidos/Ofertas ---
        // NUEVO: Obtener pedidos realizados por un cliente
        [Get("/api/PedidosOfertas/ByClient/{id_cliente}")]
        Task<ApiResponse<List<PedidoOferta>>> GetClientOrdersAsync(int id_cliente);
    }
}
