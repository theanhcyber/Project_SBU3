FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022 AS build
WORKDIR /src

COPY global.json ./
COPY TestManager.Api/TestManager.Api.csproj TestManager.Api/
RUN dotnet restore TestManager.Api/TestManager.Api.csproj

COPY . .
RUN dotnet publish TestManager.Api/TestManager.Api.csproj -c Release -o /app/publish --no-restore -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-ltsc2022 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TestManager.Api.dll"]
