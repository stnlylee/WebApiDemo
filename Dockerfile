FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish
WORKDIR /src
COPY WebApiDemo.Api/WebApiDemo.Api.csproj ./WebApiDemo.Api/WebApiDemo.Api.csproj
COPY WebApiDemo.Domain/WebApiDemo.Domain.csproj ./WebApiDemo.Domain/WebApiDemo.Domain.csproj
COPY WebApiDemo.Services/WebApiDemo.Services.csproj ./WebApiDemo.Services/WebApiDemo.Services.csproj
COPY WebApiDemo.Data/WebApiDemo.Data.csproj ./WebApiDemo.Data/WebApiDemo.Data.csproj

RUN dotnet restore "./WebApiDemo.Api/WebApiDemo.Api.csproj" --runtime alpine-x64
COPY . .
RUN dotnet publish "./WebApiDemo.Api/WebApiDemo.Api.csproj" -c Release -o /app/publish \
  --no-restore \
  --runtime alpine-x64 \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final

RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

# upgrade musl to remove potential vulnerability
RUN apk upgrade musl

USER dotnetuser
WORKDIR /app
EXPOSE 5000
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:5000
ENV ASPNETCORE_ENVIRONMENT Production

CMD ["./WebApiDemo.Api", "--urls", "http://*:5000"]