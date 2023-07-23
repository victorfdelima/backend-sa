FROM mcr.microsoft.com/dotnet/sdk:7.0 AS Build
WORKDIR /app

COPY . ./
RUN dotnet restore "./WebApi/WebApi.csproj"

# Build do projeto
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .

# Exponha a porta do servidor ASP.NET Core
EXPOSE 44363

# Comando para iniciar o backend
ENTRYPOINT ["dotnet", "WebApi.dll"]
