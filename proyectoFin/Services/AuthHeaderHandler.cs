

namespace proyectoFin.Services
{
    // DelegatingHandler permite interceptar y modificar peticiones HTTP
    public class AuthHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Intenta obtener el token JWT guardado en SecureStorage
            // La clave "jwt_token" debe coincidir con la que usas en LoginViewModel
            var token = await SecureStorage.GetAsync("jwt_token");

            // Si se encuentra un token, lo añade a la cabecera "Authorization"
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Continua con el procesamiento normal de la peticion HTTP
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
