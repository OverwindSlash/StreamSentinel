FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ["src/StreamSentinel.Web.Host/StreamSentinel.Web.Host.csproj", "src/StreamSentinel.Web.Host/"]
COPY ["src/StreamSentinel.Web.Core/StreamSentinel.Web.Core.csproj", "src/StreamSentinel.Web.Core/"]
COPY ["src/StreamSentinel.Application/StreamSentinel.Application.csproj", "src/StreamSentinel.Application/"]
COPY ["src/StreamSentinel.Core/StreamSentinel.Core.csproj", "src/StreamSentinel.Core/"]
COPY ["src/StreamSentinel.EntityFrameworkCore/StreamSentinel.EntityFrameworkCore.csproj", "src/StreamSentinel.EntityFrameworkCore/"]
WORKDIR "/src/src/StreamSentinel.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/StreamSentinel.Web.Host", "src/StreamSentinel.Web.Host"]
COPY ["src/StreamSentinel.Web.Core", "src/StreamSentinel.Web.Core"]
COPY ["src/StreamSentinel.Application", "src/StreamSentinel.Application"]
COPY ["src/StreamSentinel.Core", "src/StreamSentinel.Core"]
COPY ["src/StreamSentinel.EntityFrameworkCore", "src/StreamSentinel.EntityFrameworkCore"]
WORKDIR "/src/src/StreamSentinel.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "StreamSentinel.Web.Host.dll"]
