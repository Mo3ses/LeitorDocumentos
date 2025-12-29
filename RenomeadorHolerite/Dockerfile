# --- MUDANÇA: Usando a imagem do SDK 10.0 ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia tudo da pasta
COPY . .

# Restaura as dependências (agora com o SDK 10 vai funcionar)
RUN dotnet restore "RenomeadorHolerite.csproj" --verbosity normal

# Publica o projeto
RUN dotnet publish "RenomeadorHolerite.csproj" -c Release -o /app/publish

# --- MUDANÇA: Usando o runtime do ASP.NET 10.0 ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RenomeadorHolerite.dll"]