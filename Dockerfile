# ===============================
# Build Stage
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY *.sln .

# Copy all project files
COPY BilQalaam.Api/*.csproj BilQalaam.Api/
COPY BilQalaam.Application/*.csproj BilQalaam.Application/
COPY BilQalaam.Domain/*.csproj BilQalaam.Domain/
COPY BilQalaam.Infrastructure/*.csproj BilQalaam.Infrastructure/

# Restore
RUN dotnet restore

# Copy everything else
COPY . .

# Publish API
WORKDIR /src/BilQalaam.Api
RUN dotnet publish -c Release -o /app/publish

# ===============================
# Runtime Stage
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BilQalaam.Api.dll"]
