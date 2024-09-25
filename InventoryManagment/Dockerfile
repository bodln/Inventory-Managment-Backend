# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY InventoryManagment/*.csproj ./InventoryManagment/
WORKDIR /app/InventoryManagment
RUN dotnet restore

# Copy the rest of the application code
COPY InventoryManagment/. ./

# Publish the application
RUN dotnet publish -c Release -o out

# Stage 2: Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the output from the build stage
COPY --from=build-env /app/InventoryManagment/out .

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "InventoryManagment.dll"]
