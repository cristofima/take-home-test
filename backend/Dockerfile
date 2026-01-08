FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/Fundo.slnx .
COPY src/Fundo.Domain/Fundo.Domain.csproj Fundo.Domain/
COPY src/Fundo.Application/Fundo.Application.csproj Fundo.Application/
COPY src/Fundo.Infrastructure/Fundo.Infrastructure.csproj Fundo.Infrastructure/
COPY src/Fundo.WebApi/Fundo.WebApi.csproj Fundo.WebApi/

# Restore dependencies
RUN dotnet restore Fundo.WebApi/Fundo.WebApi.csproj

# Copy source code
COPY src/Fundo.Domain/ Fundo.Domain/
COPY src/Fundo.Application/ Fundo.Application/
COPY src/Fundo.Infrastructure/ Fundo.Infrastructure/
COPY src/Fundo.WebApi/ Fundo.WebApi/

# Build and publish
WORKDIR /src/Fundo.WebApi
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Fundo.WebApi.dll"]
