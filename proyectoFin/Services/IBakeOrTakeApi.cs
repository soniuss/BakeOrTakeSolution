using Domain.Model;
using proyectoFin.MVVM.Model.ApiRequests;
using Refit;


namespace proyectoFin.Services
{
    public interface IBakeOrTakeApi
    {
        [Get("/api/Recetas")]
        Task<ApiResponse<List<Receta>>> GetRecetasAsync();

        [Post("/api/Clientes/register")]
        Task<ApiResponse<Cliente>> RegisterClienteAsync([Body] ClienteRegistrationRequest request);

        [Post("/api/Empresas/register")]
        Task<ApiResponse<Empresa>> RegisterEmpresaAsync([Body] EmpresaRegistrationRequest request);

        [Post("/api/Auth/login")]
        Task<ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);
    }
}
