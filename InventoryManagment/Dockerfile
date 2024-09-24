FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

EXPOSE 8080

COPY InventoryManagment/*.csproj ./ 
RUN dotnet restore

COPY InventoryManagment/. ./ 
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app


COPY --from=build-env /app/out ./ 
ENTRYPOINT ["dotnet", "InventoryManagment.dll"]
