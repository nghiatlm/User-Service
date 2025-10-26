# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# set a short nuget packages path inside container to avoid any path-length issues
ENV NUGET_PACKAGES=/root/.nuget/packages

# copy solution and project files first (for layer caching)
COPY ["PRN_232-User-Service.sln", "./"]
COPY ["UserService.BO/UserService.BO.csproj", "UserService.BO/"]
COPY ["UserService.Repository/UserService.Repository.csproj", "UserService.Repository/"]
COPY ["UserService.Service/UserService.Service.csproj", "UserService.Service/"]
COPY ["UserService.API/UserService.API.csproj", "UserService.API/"]

# initial restore (will use NUGET_PACKAGES)
RUN dotnet restore "PRN_232-User-Service.sln"

# copy remaining sources
COPY . .

# ensure packages are present for publish; run restore again (safe & cheap if cache hits)
RUN dotnet restore "PRN_232-User-Service.sln"

# publish (no --no-restore to be extra-safe) 
RUN dotnet publish "UserService.API/UserService.API.csproj" -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "UserService.API.dll"]
