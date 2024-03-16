#!/bin/bash -e

# Check for the ENVIRONMENT variable and replace the file based on that
if [[ $ENVIRONMENT = "Staging" ]]; then
   cp /usr/share/nginx/html/config/app-config.staging.json /usr/share/nginx/html/app-config.json
fi

if [[ $ENVIRONMENT = "Prod" ]]; then
   cp /usr/share/nginx/html/config/app-config.prod.json /usr/share/nginx/html/app-config.json
fi

envsubst '$IP_ADDRESS' < /path/to/nginx.conf.template > /etc/nginx/nginx.conf

# Start Nginx in the foreground
nginx -g 'daemon off;'
