FROM node:lts-alpine as build

ADD ./package.json /tmp/package.json
RUN cd /tmp && npm install
RUN mkdir -p /usr/local/app && cp -a /tmp/node_modules /usr/local/app/

WORKDIR /usr/local/app

# Add the source code from the app to the container
COPY ./ /usr/local/app/

# Generate the build of the application
RUN npm run build --output-path=build

# Stage 2: Serve app with nginx server

FROM nginx:latest

# Copy the build output to replace the default nginx contents.
COPY --from=build /usr/local/app/dist/build/browser /usr/share/nginx/html

# This line is IMPORTANT, we will breakdown it on a minute.
COPY ./entrypoint.sh /usr/local/app/entrypoint.sh

# Copy the nginx conf that we created to the container
COPY ./nginx.conf  /etc/nginx/conf.d/default.conf

# Expose ports
EXPOSE 80 443

RUN chmod +x /usr/local/app/entrypoint.sh
ENTRYPOINT [ "/usr/local/app/entrypoint.sh" ]