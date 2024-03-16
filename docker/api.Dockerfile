# Specify ARG at the beginning to allow it to be passed at build time
ARG API_NAME

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish

# Re-declare ARG here because ARGs are not available across stages
ARG API_NAME

COPY . .
RUN dotnet restore "${API_NAME}" 
RUN dotnet publish "${API_NAME}/${API_NAME}.csproj" --no-restore -c Release -o /app/publish

EXPOSE 80
EXPOSE 443

#todo health endpoint
RUN apk add curl
HEALTHCHECK CMD curl --fail http://localhost:7057/${API_NAME} || exit 1

# Create a new user named nonroot with a specific UID and no password with home /app dir, and change the ownership of the /app to this user 
RUN adduser -u 5678 --disabled-password --gecos "" --home /app nonroot && chown -R nonroot /app
USER nonroot

FROM base AS final
WORKDIR /app
# RUN bash -c "apt-get update && apt-get install -y vim"
# Re-declare ARG and set it as an ENV variable for use at runtime
ARG API_NAME
ENV API_NAME=$API_NAME
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet ${API_NAME}.dll"]