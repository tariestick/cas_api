# Usamos la imagen oficial de .NET SDK para construir la app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Carpeta de trabajo dentro del contenedor
WORKDIR /app

# Copiamos los archivos del proyecto
COPY Casino_onlineAPI/*.csproj ./

# Restauramos dependencias
RUN dotnet restore

# Copiamos el resto del código
COPY . .

# Publicamos en modo Release en carpeta /app/publish
RUN dotnet publish -c Release -o /app/publish

# --- Paso final: Imagen ligera para producción ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copiamos la app publicada desde la etapa build
COPY --from=build /app/publish .

# Puerto que exponemos (ajústalo si usas otro)
EXPOSE 80

# Comando para arrancar la app
ENTRYPOINT ["dotnet", "Casino_onlineAPI.dll"]
