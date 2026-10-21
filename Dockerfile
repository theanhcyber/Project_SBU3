# Bản Dockerfile cho Docker Desktop chế độ WINDOWS containers (image nền Nano Server)
# Build:  docker build -t teamboard-api .
# Run:    docker run --rm -p 8080:8080 teamboard-api   ->  http://localhost:8080/swagger
# Bản cho Linux containers: Dockerfile.linux (docker build -f Dockerfile.linux -t teamboard-api .)
# Khác bản Linux: tag image *-nanoserver-ltsc2022, không có USER $APP_UID
# (image Nano Server mặc định đã chạy bằng user thường ContainerUser), lệnh RUN chạy bằng cmd.

# ============ Stage 1: build — image SDK ============
FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022 AS build
WORKDIR /src

# 1) Copy global.json + .csproj trước rồi restore => sửa file .cs thì bước này CACHED
COPY global.json ./
COPY TestManager.Api/TestManager.Api.csproj TestManager.Api/
RUN dotnet restore TestManager.Api/TestManager.Api.csproj

# 2) Copy toàn bộ source (đã lọc bởi .dockerignore) rồi publish
COPY . .
RUN dotnet publish TestManager.Api/TestManager.Api.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

# ============ Stage 2: runtime — image ASP.NET ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-ltsc2022 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TestManager.Api.dll"]
