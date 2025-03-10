FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TradingBot.Worker/*.csproj", "TradingBot.Worker/"]
COPY ["TradingBot.Domain/*.csproj", "TradingBot.Domain/"]
COPY ["TradingBot.Infrastructure/*.csproj", "TradingBot.Infrastructure/"]
COPY ["TradingBot.Usecases/*.csproj", "TradingBot.Usecases/"]

# Adjust paths if needed to match your actual project structure
RUN dotnet restore "TradingBot.Worker/TradingBot.Worker.csproj" -r linux-x64

# Copy the rest of the code
COPY . .

# Build the application
WORKDIR "/src/TradingBot.Worker"
RUN dotnet build "TradingBot.Worker.csproj" -c Release -o /app/build -r linux-x64

# Publish the application
FROM build AS publish
RUN dotnet publish "TradingBot.Worker.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false --no-restore

# Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingBot.Worker.dll", "--worker=PriceFetcherWorker"]