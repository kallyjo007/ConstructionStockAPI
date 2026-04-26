# Use the official .NET 10 SDK image as the build environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["ConstructionStockAPI.csproj", "./"]
RUN dotnet restore "ConstructionStockAPI.csproj"

# Copy the remaining files and build the application
COPY . .
RUN dotnet publish "ConstructionStockAPI.csproj" -c Release -o /app/publish

# Use the official ASP.NET Core Runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Tell ASP.NET Core to bind globally on Port 80 (Render supports this securely)
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Start the API
ENTRYPOINT ["dotnet", "ConstructionStockAPI.dll"]
