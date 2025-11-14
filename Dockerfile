# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file
COPY ["Energix.API.csproj", "./"]
RUN dotnet restore "Energix.API.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "Energix.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Energix.API.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Energix.API.dll"]