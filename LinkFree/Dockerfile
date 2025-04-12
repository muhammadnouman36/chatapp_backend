# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["LinkFree/LinkFree.csproj", "LinkFree/"]
RUN dotnet restore "LinkFree/LinkFree.csproj"

# Copy the entire source code
COPY . .

# Build the project
WORKDIR "/src/LinkFree"
RUN dotnet build "LinkFree.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkFree.csproj" -c Release -o /app/publish

# Copy the build output to the base image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "LinkFree.dll"]
