# 1. Imagem de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- MUDANÇA: Copia o arquivo EXATO. Se falhar aqui, é erro de caminho ---
COPY RenomeadorHolerite.csproj .

# --- MUDANÇA: Restore com mais detalhes para vermos o erro real se falhar ---
RUN dotnet restore RenomeadorHolerite.csproj --verbosity detailed

# Copia todo o resto
COPY . .

# Publica a aplicação
RUN dotnet publish -c Release -o /app/publish

# 2. Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RenomeadorHolerite.dll"]