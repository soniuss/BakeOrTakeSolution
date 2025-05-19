FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet workload restore
RUN dotnet restore BakeOrTakeSolution.sln
RUN dotnet publish Persistence.ApiRest/Persistence.ApiRest.csproj -c Release -o /app/publish
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef database update --project Persistence.ApiRest/Persistence.ApiRest.csproj --startup-project Persistence.ApiRest/Persistence.ApiRest.csproj --solution BakeOrTakeSolution.sln --no-build --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 8080
ENTRYPOINT ["dotnet", "Persistence.ApiRest.dll"]
