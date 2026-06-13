# Estructura multi-etapa para optimizar tamaño y seguridad

# Etapa 1: Build de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar archivos de proyectos para restaurar dependencias
COPY src/ConsultoraPro.API/ConsultoraPro.API.csproj src/ConsultoraPro.API/
COPY src/ConsultoraPro.Application/ConsultoraPro.Application.csproj src/ConsultoraPro.Application/
COPY src/ConsultoraPro.Domain/ConsultoraPro.Domain.csproj src/ConsultoraPro.Domain/
COPY src/ConsultoraPro.Infrastructure/ConsultoraPro.Infrastructure.csproj src/ConsultoraPro.Infrastructure/

# Restaurar dependencias
RUN dotnet restore src/ConsultoraPro.API/ConsultoraPro.API.csproj

# Copiar el resto del código y compilar
COPY src/ src/
RUN dotnet publish src/ConsultoraPro.API/ConsultoraPro.API.csproj -c Release -o out

# Etapa 2: Imagen de ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Exponer el puerto por defecto de ASP.NET Core
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicio
ENTRYPOINT ["dotnet", "ConsultoraPro.API.dll"]
