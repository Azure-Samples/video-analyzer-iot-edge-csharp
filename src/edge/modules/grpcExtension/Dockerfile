FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .

RUN dotnet restore "grpcExtension.csproj"
RUN dotnet build "grpcExtension.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "grpcExtension.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Required for using System.Drawing.Common
RUN apt-get update -y && \
    apt-get install -y --no-install-recommends libgdiplus

ENTRYPOINT ["dotnet", "GrpcExtension.dll"]