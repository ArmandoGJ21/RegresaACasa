# Imagen de la API (compilación en dos etapas).
#   docker build -t regresaacasa-api ./backend
# La configuración (.env, llaves) NO se copia a la imagen: se pasa con variables de entorno.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar primero solo con el .csproj para aprovechar la caché de Docker.
COPY src/RegresaACasa.Api/RegresaACasa.Api.csproj src/RegresaACasa.Api/
RUN dotnet restore src/RegresaACasa.Api/RegresaACasa.Api.csproj

COPY src/ src/
RUN dotnet publish src/RegresaACasa.Api/RegresaACasa.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Npgsql intenta cargar la librería de Kerberos (GSSAPI); sin ella escribe un error en el log.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Usuario sin privilegios que trae la imagen oficial de .NET.
USER $APP_UID
ENTRYPOINT ["dotnet", "RegresaACasa.Api.dll"]
