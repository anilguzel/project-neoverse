FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/DocumentManagement/Neoverse.DocumentManagement.Api/Neoverse.DocumentManagement.Api.csproj", "src/Services/DocumentManagement/Neoverse.DocumentManagement.Api/"]
COPY ["src/Services/DocumentManagement/Neoverse.DocumentManagement.Infrastructure/Neoverse.DocumentManagement.Infrastructure.csproj", "src/Services/DocumentManagement/Neoverse.DocumentManagement.Infrastructure/"]
COPY ["src/Services/DocumentManagement/Neoverse.DocumentManagement.Application/Neoverse.DocumentManagement.Application.csproj", "src/Services/DocumentManagement/Neoverse.DocumentManagement.Application/"]
COPY ["src/Services/DocumentManagement/Neoverse.DocumentManagement.Domain/Neoverse.DocumentManagement.Domain.csproj", "src/Services/DocumentManagement/Neoverse.DocumentManagement.Domain/"]
COPY ["src/Neoverse.SharedKernel/Neoverse.SharedKernel.csproj", "src/Neoverse.SharedKernel/"]
COPY ["src/Neoverse.ApiBase/Neoverse.ApiBase.csproj", "src/Neoverse.ApiBase/"]
COPY ["src/Services/Customers/Neoverse.Customers.Api/Neoverse.Customers.Api.csproj", "src/Services/Customers/Neoverse.Customers.Api/"]
COPY ["src/Services/Customers/Neoverse.Customers.Domain/Neoverse.Customers.Domain.csproj", "src/Services/Customers/Neoverse.Customers.Domain/"]
COPY ["src/Services/Customers/Neoverse.Customers.Application/Neoverse.Customers.Application.csproj", "src/Services/Customers/Neoverse.Customers.Application/"]
COPY ["src/Services/Customers/Neoverse.Customers.Infrastructure/Neoverse.Customers.Infrastructure.csproj", "src/Services/Customers/Neoverse.Customers.Infrastructure/"]
RUN dotnet restore "src/Services/DocumentManagement/Neoverse.DocumentManagement.Api/Neoverse.DocumentManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/DocumentManagement/Neoverse.DocumentManagement.Api"
RUN dotnet build "./Neoverse.DocumentManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Neoverse.DocumentManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neoverse.DocumentManagement.Api.dll"]
