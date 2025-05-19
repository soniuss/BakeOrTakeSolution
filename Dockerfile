# Etapa de build
# Usamos una imagen con el SDK de .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Establecemos el directorio de trabajo dentro del contenedor
WORKDIR /src

# Copiamos el archivo de solución primero. Esto es importante para dotnet restore/publish
COPY BakeOrTakeSolution.sln .

# Copiamos los archivos .csproj de los proyectos que necesitamos para restaurar
# Las rutas son relativas al directorio de trabajo (/src) y a la raíz del contexto de build (donde esta el Dockerfile)
COPY Domain.Model/Domain.Model.csproj Domain.Model/
COPY Persistence.ApiRest/Persistence.ApiRest.csproj Persistence.ApiRest/

# Restauramos las dependencias usando el archivo de solución.
# Esto asegura que se resuelvan las referencias entre proyectos.
RUN dotnet restore BakeOrTakeSolution.sln

# Copiamos el resto de los archivos del proyecto
# Las rutas son relativas a la raíz del contexto de build y se copian a /src
COPY . .

# Publicamos el proyecto API
# La ruta del proyecto es relativa al directorio de trabajo (/src)
RUN dotnet publish Persistence.ApiRest/Persistence.ApiRest.csproj -c Release -o /app/publish

# Etapa de runtime
# Usamos una imagen base mas ligera para ejecutar la aplicacion
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Establecemos el directorio de trabajo final
WORKDIR /app

# Copiamos los archivos publicados desde la etapa de build
COPY --from=build /app/publish .

# Configuramos el puerto que Railway inyecta por variable de entorno ($PORT)
# La aplicacion .NET por defecto escucha en el puerto 80 (o 443 para https)
# Mapeamos el puerto 8080 del contenedor al puerto $PORT del host (Railway lo hace)
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080 # ¡Comentario ELIMINADO de esta linea!

# Definimos el punto de entrada de la aplicacion
ENTRYPOINT ["dotnet", "Persistence.ApiRest.dll"]
