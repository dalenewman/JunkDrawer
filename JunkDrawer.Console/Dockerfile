ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/runtime:10.0
ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0

FROM --platform=$BUILDPLATFORM ${SDK_IMAGE} AS build
WORKDIR /src
COPY . .
RUN dotnet publish JunkDrawer.Console/JunkDrawer.Console.csproj -c Release -o /app/publish

FROM ${RUNTIME_IMAGE}
WORKDIR /data
COPY --from=build /app/publish /app
ENTRYPOINT ["dotnet", "/app/jd.dll"]
