# Use the official .NET image as a base image
# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChatApp/ChatApp.csproj", "ChatApp/"]
RUN dotnet restore "ChatApp/ChatApp.csproj"
COPY . .
WORKDIR "/src/ChatApp"
RUN dotnet build "ChatApp.csproj" -c Release -o /app/build

# Stage 3: Publish the app to the /app/publish directory
FROM build AS publish
RUN dotnet publish "ChatApp.csproj" -c Release -o /app/publish

# Stage 4: Final stage to set up the runtime environment
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.dll"]
