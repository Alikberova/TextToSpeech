#!/bin/bash -e

# Check for the ENVIRONMENT variable and replace the file based on that
if [[ $ENVIRONMENT = "Staging" ]]; then
   cp /usr/share/nginx/html/config/app-config.staging.json /usr/share/nginx/html/app-config.json
fi

if [[ $ENVIRONMENT = "Production" ]]; then
   cp /usr/share/nginx/html/config/app-config.prod.json /usr/share/nginx/html/app-config.json
fi

# Copy Angular build output to the Nginx container
docker cp booktoaudioweb:/usr/share/nginx/html/ ./certbot/www/
