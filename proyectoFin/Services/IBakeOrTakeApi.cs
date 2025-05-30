using Domain.Model;
using Domain.Model.ApiRequests;
using Refit;


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
    }
}
