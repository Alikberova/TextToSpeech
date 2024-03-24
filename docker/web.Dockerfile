FROM node:lts-alpine as build

WORKDIR /src
COPY package.json /src/package.json
RUN npm install

# Copy the rest of the source code
COPY . /src

# Build the Angular app
RUN npm run build

# COPY "/config/${ENVIRONMENT}-app-config.json" "/usr/share/nginx/html/app-config.json"

# # Fix and copy the entrypoint script
# COPY entrypoint.sh /entrypoint.sh
# RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# ENTRYPOINT ["/entrypoint.sh"]