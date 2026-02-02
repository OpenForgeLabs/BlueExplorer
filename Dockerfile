# syntax=docker/dockerfile:1

FROM node:20-alpine AS web-build
WORKDIR /app
COPY Apps/blueexplorer-web/package.json ./package.json
RUN npm install --no-audit --no-fund
COPY Apps/blueexplorer-web/ .
RUN npm run build

FROM node:20-alpine AS web-runtime
WORKDIR /app
ENV NODE_ENV=production
COPY --from=web-build /app/.next ./.next
COPY --from=web-build /app/public ./public
COPY --from=web-build /app/package.json ./package.json
COPY --from=web-build /app/node_modules ./node_modules
EXPOSE 3000
CMD ["npm", "start"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS servicebus-build
WORKDIR /src
COPY Services/ ./Services/
RUN dotnet restore Services/AzureServiceBus/ServiceBus.Api/ServiceBus.Api.csproj
RUN dotnet publish Services/AzureServiceBus/ServiceBus.Api/ServiceBus.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS servicebus-runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=servicebus-build /app/publish .
ENTRYPOINT ["dotnet", "ServiceBus.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS redis-build
WORKDIR /src
COPY Services/ ./Services/
RUN dotnet restore Services/AzureRedis/AzureRedis.Api/AzureRedis.Api.csproj
RUN dotnet publish Services/AzureRedis/AzureRedis.Api/AzureRedis.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS redis-runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=redis-build /app/publish .
ENTRYPOINT ["dotnet", "AzureRedis.Api.dll"]
