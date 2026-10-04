FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.24 AS build
ARG TARGETARCH
WORKDIR /src

COPY .editorconfig global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY src/PhoneBook.Api/PhoneBook.Api.csproj src/PhoneBook.Api/
RUN dotnet restore src/PhoneBook.Api/PhoneBook.Api.csproj -a "$TARGETARCH"

COPY src/PhoneBook.Api/ src/PhoneBook.Api/
RUN dotnet publish src/PhoneBook.Api/PhoneBook.Api.csproj \
    -c Release \
    -a "$TARGETARCH" \
    --no-restore \
    -p:UseAppHost=false \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12-alpine3.24 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish ./
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=60s --retries=6 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "PhoneBook.Api.dll"]
