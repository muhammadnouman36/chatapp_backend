# STEP 1: Use base image for .NET 8 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# STEP 2: Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything to the container
COPY . .

# Restore and publish the WebApi project
WORKDIR /src/LinkFree
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# STEP 3: Final image with only runtime dependencies
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LinkFree.dll"]
