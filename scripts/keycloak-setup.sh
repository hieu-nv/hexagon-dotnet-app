#!/bin/bash

# Keycloak Setup Script for Hexagon .NET App
# This script sets up a Keycloak realm, client, and test users for SAML2 authentication

set -e

KEYCLOAK_URL="http://localhost:8080"
ADMIN_USER="admin"
ADMIN_PASSWORD="admin"
REALM_NAME="hexagon"
CLIENT_ID="hexagon-app"
CLIENT_ASSERTION_CONSUMER_SERVICE_URL="https://localhost:5112/saml/acs"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Wait for Keycloak to be ready
print_info "Waiting for Keycloak to be ready..."
max_attempts=30
attempt=0
until curl -s "$KEYCLOAK_URL/health" > /dev/null 2>&1 || [ "$attempt" -ge "$max_attempts" ]; do
    attempt=$((attempt + 1))
    echo -n "."
    sleep 2
done
echo ""

if [ "$attempt" -ge "$max_attempts" ]; then
    print_error "Keycloak failed to start within timeout"
    exit 1
fi

print_info "Keycloak is ready!"

# Get admin access token
print_info "Authenticating with Keycloak..."
TOKEN_RESPONSE=$(curl -s -X POST \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=password&client_id=admin-cli&username=$ADMIN_USER&password=$ADMIN_PASSWORD" \
    "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token")

ACCESS_TOKEN=$(echo "$TOKEN_RESPONSE" | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ]; then
    print_error "Failed to obtain admin token"
    echo "$TOKEN_RESPONSE"
    exit 1
fi

print_info "Successfully authenticated"

# Create realm
print_info "Creating realm: $REALM_NAME"
curl -s -X POST \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "realm": "'$REALM_NAME'",
        "displayName": "Hexagon",
        "enabled": true,
        "accessTokenLifespan": 3600,
        "refreshTokenLifespan": 7200
    }' \
    "$KEYCLOAK_URL/admin/realms"

print_info "Realm created successfully"

# Create SAML client
print_info "Creating SAML client: $CLIENT_ID"
curl -s -X POST \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "clientId": "'$CLIENT_ID'",
        "name": "Hexagon .NET App",
        "protocol": "saml",
        "enabled": true,
        "clientAuthenticatorType": "client-secret",
        "redirectUris": [
            "'$CLIENT_ASSERTION_CONSUMER_SERVICE_URL'"
        ],
        "attributes": {
            "saml.assertion.signature": "true",
            "saml.server.signature": "true",
            "saml.client.signature": "false",
            "saml.encryption.certificate": "",
            "saml.sp.metadata.attribute.consuming.service.index": "0",
            "saml_assertion_consumer_url_post": "'$CLIENT_ASSERTION_CONSUMER_SERVICE_URL'",
            "saml_assertion_consumer_url_redirect": "'$CLIENT_ASSERTION_CONSUMER_SERVICE_URL'",
            "saml_single_logout_service_url_post": "https://localhost:5112/saml/logout",
            "saml_single_logout_service_url_redirect": "https://localhost:5112/saml/logout"
        }
    }' \
    "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients"

print_info "SAML client created successfully"

# Create realm roles
print_info "Creating roles..."
for role in admin user; do
    curl -s -X POST \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"name": "'$role'", "description": "'$role' role"}' \
        "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles"
    print_info "Role '$role' created"
done

# Create test users
print_info "Creating test users..."

# Admin user
curl -s -X POST \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "admin@example.com",
        "email": "admin@example.com",
        "attributes": {
            "name": ["Admin User"]
        },
        "enabled": true,
        "credentials": [
            {
                "type": "password",
                "value": "admin123",
                "temporary": false
            }
        ],
        "realmRoles": ["admin", "user"]
    }' \
    "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users"

print_info "Admin user created: admin@example.com / admin123"

# Regular user
curl -s -X POST \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "user@example.com",
        "email": "user@example.com",
        "attributes": {
            "name": ["Regular User"]
        },
        "enabled": true,
        "credentials": [
            {
                "type": "password",
                "value": "user123",
                "temporary": false
            }
        ],
        "realmRoles": ["user"]
    }' \
    "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users"

print_info "Regular user created: user@example.com / user123"

echo ""
print_info "============================================"
print_info "Keycloak setup completed successfully!"
print_info "============================================"
print_info "Realm URL: $KEYCLOAK_URL/realms/$REALM_NAME"
print_info "Admin Console: $KEYCLOAK_URL/admin/master/console"
print_info "Realm: $REALM_NAME"
print_info "Admin Username: $ADMIN_USER"
print_info "Admin Password: $ADMIN_PASSWORD"
print_info ""
print_info "Test Users:"
print_info "  - admin@example.com / admin123 (roles: admin, user)"
print_info "  - user@example.com / user123 (role: user)"
print_info ""
print_warning "Update your application's appsettings.json with Keycloak details"
echo ""
