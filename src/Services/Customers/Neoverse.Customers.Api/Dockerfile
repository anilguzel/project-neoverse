FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Customers/Neoverse.Customers.Api/Neoverse.Customers.Api.csproj", "src/Services/Customers/Neoverse.Customers.Api/"]
COPY ["src/Services/Customers/Neoverse.Customers.Domain/Neoverse.Customers.Domain.csproj", "src/Services/Customers/Neoverse.Customers.Domain/"]
COPY ["src/Services/Customers/Neoverse.Customers.Application/Neoverse.Customers.Application.csproj", "src/Services/Customers/Neoverse.Customers.Application/"]
COPY ["src/Services/Customers/Neoverse.Customers.Infrastructure/Neoverse.Customers.Infrastructure.csproj", "src/Services/Customers/Neoverse.Customers.Infrastructure/"]
COPY ["src/Neoverse.ApiBase/Neoverse.ApiBase.csproj", "src/Neoverse.ApiBase/"]
COPY ["src/Neoverse.SharedKernel/Neoverse.SharedKernel.csproj", "src/Neoverse.SharedKernel/"]

RUN dotnet restore "src/Services/Customers/Neoverse.Customers.Api/Neoverse.Customers.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Customers/Neoverse.Customers.Api"
RUN dotnet build "./Neoverse.Customers.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neoverse.Customers.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neoverse.Customers.Api.dll"]
