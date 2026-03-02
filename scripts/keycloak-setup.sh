#!/bin/bash
# scripts/keycloak-setup.sh
# Sets up the Hexagon Keycloak realm, clients, and test users.

set -e

KEYCLOAK_URL="http://localhost:8080"
ADMIN_USER="admin"
ADMIN_PASS="admin"
REALM_NAME="hexagon"
CLIENT_ID="hexagon-api"

echo "Waiting for Keycloak to be ready..."
while ! curl -s $KEYCLOAK_URL/health/ready > /dev/null; do
    echo "Keycloak not ready, waiting 2 seconds..."
    sleep 2
done

echo "Keycloak is ready!"

# 1. Get Admin Token
echo "Authenticating as admin..."
ADMIN_TOKEN=$(curl -s -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=$ADMIN_USER" \
    -d "password=$ADMIN_PASS" \
    -d "grant_type=password" \
    -d "client_id=admin-cli" | jq -r .access_token)

if [ "$ADMIN_TOKEN" == "null" ] || [ -z "$ADMIN_TOKEN" ]; then
    echo "Failed to get admin token!"
    exit 1
fi

# 2. Check if realm exists
REALM_EXISTS=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME" \
    -H "Authorization: Bearer $ADMIN_TOKEN" -o /dev/null -w "%{http_code}")

if [ "$REALM_EXISTS" == "200" ]; then
    echo "Realm $REALM_NAME already exists. Skipping setup."
    exit 0
fi

# 3. Create Realm
echo "Creating realm: $REALM_NAME..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "realm": "'$REALM_NAME'",
        "enabled": true,
        "registrationAllowed": false,
        "resetPasswordAllowed": false,
        "rememberMe": false,
        "verifyEmail": false,
        "accessTokenLifespan": 3600,
        "ssoSessionIdleTimeout": 18000,
        "ssoSessionMaxLifespan": 36000,
        "sslRequired": "external"
    }' > /dev/null

# 4. Create Client
echo "Creating client: $CLIENT_ID..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "clientId": "'$CLIENT_ID'",
        "enabled": true,
        "clientAuthenticatorType": "client-secret",
        "publicClient": true,
        "standardFlowEnabled": true,
        "implicitFlowEnabled": false,
        "directAccessGrantsEnabled": true,
        "serviceAccountsEnabled": false,
        "redirectUris": ["http://localhost:5112/*", "https://localhost:7153/*"],
        "webOrigins": ["+"],
        "fullScopeAllowed": true
    }' > /dev/null

# 5. Create Roles
echo "Creating roles..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"name": "admin", "description": "Administrator role"}' > /dev/null

curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"name": "user", "description": "Standard user role"}' > /dev/null

# Get Role IDs
ADMIN_ROLE_ID=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles/admin" -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r .id)
USER_ROLE_ID=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles/user" -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r .id)

# 6. Create Users
echo "Creating users..."

# Admin User
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "admin@example.com",
        "email": "admin@example.com",
        "firstName": "Admin",
        "lastName": "User",
        "enabled": true,
        "emailVerified": true,
        "credentials": [{"type": "password", "value": "password", "temporary": false}]
    }' > /dev/null

# Regular User
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "username": "user@example.com",
        "email": "user@example.com",
        "firstName": "Standard",
        "lastName": "User",
        "enabled": true,
        "emailVerified": true,
        "credentials": [{"type": "password", "value": "password", "temporary": false}]
    }' > /dev/null

# Get User IDs
ADMIN_USER_ID=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users?username=admin@example.com" -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r .[0].id)
REGULAR_USER_ID=$(curl -s -X GET "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users?username=user@example.com" -H "Authorization: Bearer $ADMIN_TOKEN" | jq -r .[0].id)

# 7. Assign Mapping Roles to Users
echo "Assigning roles to users..."

# Assign 'admin' and 'user' to admin@example.com
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users/$ADMIN_USER_ID/role-mappings/realm" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '[
        {"id": "'$ADMIN_ROLE_ID'", "name": "admin"},
        {"id": "'$USER_ROLE_ID'", "name": "user"}
    ]' > /dev/null

# Assign 'user' to user@example.com
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users/$REGULAR_USER_ID/role-mappings/realm" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '[
        {"id": "'$USER_ROLE_ID'", "name": "user"}
    ]' > /dev/null


echo "=========================================="
echo "Keycloak setup completed successfully!"
echo "Realm: $REALM_NAME"
echo "Client: $CLIENT_ID"
echo "Admin User: admin@example.com / password"
echo "Standard User: user@example.com / password"
echo "=========================================="
