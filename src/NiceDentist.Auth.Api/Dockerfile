# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj files first to leverage layer caching
COPY ./src/NiceDentist.Auth.Domain/NiceDentist.Auth.Domain.csproj ./NiceDentist.Auth.Domain/
COPY ./src/NiceDentist.Auth.Application/NiceDentist.Auth.Application.csproj ./NiceDentist.Auth.Application/
COPY ./src/NiceDentist.Auth.Infrastructure/NiceDentist.Auth.Infrastructure.csproj ./NiceDentist.Auth.Infrastructure/
COPY ./src/NiceDentist.Auth.Api/NiceDentist.Auth.Api.csproj ./NiceDentist.Auth.Api/

RUN dotnet restore ./NiceDentist.Auth.Api/NiceDentist.Auth.Api.csproj

# copy the rest of the source
COPY ./src/NiceDentist.Auth.Domain/ ./NiceDentist.Auth.Domain/
COPY ./src/NiceDentist.Auth.Application/ ./NiceDentist.Auth.Application/
COPY ./src/NiceDentist.Auth.Infrastructure/ ./NiceDentist.Auth.Infrastructure/
COPY ./src/NiceDentist.Auth.Api/ ./NiceDentist.Auth.Api/

RUN dotnet publish ./NiceDentist.Auth.Api/NiceDentist.Auth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NiceDentist.Auth.Api.dll"]
