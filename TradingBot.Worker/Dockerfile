FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TradingBot.Worker/*.csproj", "TradingBot.Worker/"]
COPY ["TradingBot.Domain/*.csproj", "TradingBot.Domain/"]
COPY ["TradingBot.Infrastructure/*.csproj", "TradingBot.Infrastructure/"]
COPY ["TradingBot.Usecases/*.csproj", "TradingBot.Usecases/"]

# Adjust paths if needed to match your actual project structure
RUN dotnet restore "TradingBot.Worker/TradingBot.Worker.csproj"

# Copy the rest of the code
COPY . .

# Build the application
WORKDIR "/src/TradingBot.Worker"
RUN dotnet build "TradingBot.Worker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TradingBot.Worker.csproj" -c Release -o /app/publish

# Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingBot.Worker.dll"]