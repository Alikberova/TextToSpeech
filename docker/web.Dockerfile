FROM node:lts-alpine as build

WORKDIR /src
COPY package.json /src/package.json
RUN npm install

# Copy the rest of the source code
COPY . /src

# Build the Angular app
RUN npm run build

# Copy the build output to replace the default Nginx contents
COPY --from=build /src/dist/book-to-audio.web/browser /usr/share/nginx/html

# Fix and copy the entrypoint script
COPY entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# RUN bash -c "apt-get update && apt-get install -y vim"

ENTRYPOINT ["/entrypoint.sh"]