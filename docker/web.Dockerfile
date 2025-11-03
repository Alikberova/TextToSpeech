FROM node:25.1.0-alpine AS build

WORKDIR /src
COPY package*.json /src/
RUN npm install

# Copy the rest of the source code
COPY . /src
# Build the Angular app
RUN npm run build

# Stage 2: Serve the app with Nginx
FROM nginx:stable-alpine

# Copy the build output to replace the default Nginx contents
COPY --from=build /src/dist/text-to-speech.web/browser /usr/share/nginx/html

# # Fix and copy the entrypoint script
COPY entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Copy the Nginx configuration
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf

# Install Nano
RUN apk update && apk add nano

ENTRYPOINT ["/entrypoint.sh"]