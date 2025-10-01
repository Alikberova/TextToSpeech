#!/bin/sh

set -e

TOKEN_FILE="/token/kibana_token"

if [ -f "$TOKEN_FILE" ]; then
    echo "Kibana token already exists, skipping creation."
    exit 0
fi

RESPONSE=$(curl -s -X POST "http://elasticsearch:9200/_security/service/elastic/kibana/credential/token/kbn-default-test" \
  -H "Content-Type: application/json" \
  -u "elastic:${ELASTIC_PASSWORD}")

TOKEN=$(echo "$RESPONSE" | grep -o '"value":"[^"]*' | cut -d':' -f2 | tr -d '"')

if [ -z "$TOKEN" ]; then
    echo "Failed to create token. API Response: $RESPONSE"
    exit 1
fi

# Save the token to the shared volume
echo -n "$TOKEN" > "$TOKEN_FILE"
echo "Token created successfully and saved to $TOKEN_FILE"