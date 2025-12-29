# Usa a imagem oficial do .NET para construir o projeto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["RenomeadorHolerite.csproj", "./"]
RUN dotnet restore "RenomeadorHolerite.csproj"

# Copia todo o resto do código e compila
COPY . .
RUN dotnet publish "RenomeadorHolerite.csproj" -c Release -o /app/publish

# Cria a imagem final leve apenas para rodar
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# O Render usa a variável PORT para definir a porta
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RenomeadorHolerite.dll"]