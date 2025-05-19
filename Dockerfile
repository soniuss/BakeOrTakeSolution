FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BakeOrTakeSolution.sln .
COPY Domain.Model/Domain.Model.csproj Domain.Model/
COPY Persistence.ApiRest/Persistence.ApiRest.csproj Persistence.ApiRest/
RUN dotnet restore BakeOrTakeSolution.sln
COPY . .
RUN dotnet publish Persistence.ApiRest/Persistence.ApiRest.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080
ENTRYPOINT ["dotnet", "Persistence.ApiRest.dll"]

