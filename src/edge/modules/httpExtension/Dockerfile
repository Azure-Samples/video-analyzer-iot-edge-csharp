FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim

# Required for using System.Drawing.Common
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends libgdiplus

WORKDIR /app
COPY --from=build-env /app/out ./

RUN useradd -ms /bin/bash moduleuser
USER moduleuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "httpExtension.dll"]