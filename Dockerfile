# ===============================
# Build Stage
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution
COPY BilQalaam.sln .

# copy csproj files
COPY BilQalaam/BilQalaam.csproj BilQalaam/
COPY BilQalaam.Application/BilQalaam.Application.csproj BilQalaam.Application/
COPY BilQalaam.Domain/BilQalaam.Domain.csproj BilQalaam.Domain/
COPY BilQalaam.Infrastructure/BilQalaam.Infrastructure.csproj BilQalaam.Infrastructure/

# restore
RUN dotnet restore

# copy everything
COPY . .

# build & publish
RUN dotnet publish BilQalaam/BilQalaam.csproj -c Release -o /app/publish

# ===============================
# Runtime Stage
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BilQalaam.dll"]
