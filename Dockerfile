FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OptiDrive.sln ./
COPY OptiDrive.Web/OptiDrive.Web.csproj OptiDrive.Web/
COPY OptiDrive.Web.Tests/OptiDrive.Web.Tests.csproj OptiDrive.Web.Tests/
RUN dotnet restore OptiDrive.sln

COPY . .
RUN dotnet publish OptiDrive.Web/OptiDrive.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
RUN mkdir -p /app/data
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OptiDrive.Web.dll"]
