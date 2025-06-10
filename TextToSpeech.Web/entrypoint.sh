#!/bin/sh

# Check for the ASPNETCORE_ENVIRONMENT variable and replace the file based on that
if [[ $ASPNETCORE_ENVIRONMENT = "Staging" ]]; then
   cp /usr/share/nginx/html/config/app-config.staging.json /usr/share/nginx/html/app-config.json
fi

if [[ $ASPNETCORE_ENVIRONMENT = "Production" ]]; then
   cp /usr/share/nginx/html/config/app-config.prod.json /usr/share/nginx/html/app-config.json
fi

# Start Nginx in the foreground
nginx -g 'daemon off;'
