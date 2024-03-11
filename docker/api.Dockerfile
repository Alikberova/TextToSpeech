# Specify ARG at the beginning to allow it to be passed at build time
ARG API_NAME

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /src

# Re-declare ARG here because ARGs are not available across stages
ARG API_NAME
COPY . .
RUN dotnet restore "${API_NAME}"

WORKDIR "/src/${API_NAME}"
RUN dotnet publish --no-restore -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# Re-declare ARG and set it as an ENV variable for use at runtime
ARG API_NAME
ENV API_NAME=$API_NAME
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet ${API_NAME}.dll"]