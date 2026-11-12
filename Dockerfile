# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["CropAdvisory.csproj", "./"]
RUN dotnet restore "CropAdvisory.csproj"

# Copy full source and publish
COPY . .
RUN dotnet publish "CropAdvisory.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy compiled output from build stage
COPY --from=build /app/publish .

# Expose standard container ports
EXPOSE 8080
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

ENTRYPOINT ["dotnet", "CropAdvisory.dll"]
