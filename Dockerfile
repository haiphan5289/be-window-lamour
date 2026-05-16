FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Lamour.sln .
COPY src/Lamour.Api/Lamour.Api.csproj src/Lamour.Api/
COPY src/Lamour.Application/Lamour.Application.csproj src/Lamour.Application/
COPY src/Lamour.Domain/Lamour.Domain.csproj src/Lamour.Domain/
COPY src/Lamour.Infrastructure/Lamour.Infrastructure.csproj src/Lamour.Infrastructure/

RUN dotnet restore

COPY . .
RUN dotnet publish src/Lamour.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lamour.Api.dll"]
