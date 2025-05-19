# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos y restauramos dependencias solo de la API
COPY Persistence.ApiRest/Persistence.ApiRest.csproj Persistence.ApiRest/
COPY Domain.Model/Domain.Model.csproj Domain.Model/
RUN dotnet restore Persistence.ApiRest/Persistence.ApiRest.csproj

# Copiamos el resto
COPY . .

RUN dotnet publish Persistence.ApiRest/Persistence.ApiRest.csproj -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Puerto que Railway inyecta por variable de entorno
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080

ENTRYPOINT ["dotnet", "Persistence.ApiRest.dll"]
