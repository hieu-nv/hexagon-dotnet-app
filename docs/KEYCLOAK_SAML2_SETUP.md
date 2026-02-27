# Keycloak SAML2 Setup Guide

This guide explains how to set up Keycloak as a SAML2 Identity Provider for the Hexagon .NET application.

## Quick Start

### 1. Start Keycloak with Podman

```bash
# Start Keycloak container using podman-compose
podman-compose -f podman-compose.yml up -d

# Verify it's running
podman ps | grep keycloak
```

Keycloak will be available at: http://localhost:8080

### 2. Run the Setup Script

```bash
# Make the script executable (if not already done)
chmod +x scripts/keycloak-setup.sh

# Run the automated setup
./scripts/keycloak-setup.sh
```

This script will:

- Create the `hexagon` realm
- Configure the SAML client `hexagon-app`
- Create test users with roles

After running the script, you'll see output with test credentials.

### 3. Verify Configuration

1. Open http://localhost:8080
2. Log in with: `admin` / `admin`
3. Navigate to Realm Settings → Realms → `hexagon`
4. Check the created realm and client configuration

## Configuration Details

### Keycloak Realm: `hexagon`

```
Name: hexagon
Realm URL: http://localhost:8080/realms/hexagon
```

### SAML Client: `hexagon-app`

```
Client ID: hexagon-app
Protocol: SAML
ACS URL: https://localhost:5112/saml/acs
Single Logout URL: https://localhost:5112/saml/logout
```

### Test Users

| Email             | Password | Roles       |
| ----------------- | -------- | ----------- |
| admin@example.com | admin123 | admin, user |
| user@example.com  | user123  | user        |

## Key SAML Endpoints

Once Keycloak is set up, these endpoints are available:

- **SAML Login**: The .NET app will redirect unauthenticated users to Keycloak
- **Metadata**: `http://localhost:8080/realms/hexagon/protocol/saml/descriptor`
- **SSO Endpoint**: `http://localhost:8080/realms/hexagon/protocol/saml`

## Application Configuration

Update your `appsettings.json`:

```json
{
  "Keycloak": {
    "Realm": "hexagon",
    "AuthServer": "http://localhost:8080",
    "ClientId": "hexagon-app",
    "ValidateSSLCertificate": false
  }
}
```

For development with self-signed certificates, SSL validation is disabled. **Always enable in production.**

## Troubleshooting

### Keycloak won't start

```bash
# Check logs
podman logs hexagon-keycloak

# Ensure port 8080 is free
lsof -i :8080
```

### Setup script fails

```bash
# Verify Keycloak is running and healthy
curl -f http://localhost:8080/health

# Check credentials
curl -X POST \
  -d "grant_type=password&client_id=admin-cli&username=admin&password=admin" \
  http://localhost:8080/realms/master/protocol/openid-connect/token
```

### SAML Login Fails

1. Verify Keycloak is accessible from the app
2. Check SAML assertion is being signed by Keycloak
3. Review .NET app logs for SAML validation errors

## Advanced Configuration

### Keycloak Admin Console

Access the admin console at: http://localhost:8080/admin/master/console

From here you can:

- Manage users and assign roles
- Configure SAML attributes and mappings
- Set up custom mappers for claims
- Configure email verification, password policies, etc.

### Custom SAML Attributes

To add custom attributes to SAML assertions:

1. Go to Realm → Clients → hexagon-app → Mappers
2. Create New Mapper
3. Example: Map Keycloak groups to SAML attributes

```
Name: Groups Mapper
Mapper Type: Group list
Token Claim Name: groups
Full group path: On
Add to SAML attribute name: groups
```

### SSL/TLS Settings

For production:

```bash
# Generate self-signed cert
openssl req -x509 -newkey rsa:4096 -keyout keycloak.key -out keycloak.crt -days 365 -nodes

# Update podman-compose.yml volumes
volumes:
  - ./keycloak.crt:/etc/x509/https/tls.crt
  - ./keycloak.key:/etc/x509/https/tls.key

# Enable HTTPS
KC_HTTPS_ENABLED: true
KC_HTTPS_PORT: 8443
```

## Cleanup

To remove Keycloak and data:

```bash
# Stop containers
podman-compose -f podman-compose.yml down

# Remove data volume
podman volume rm keycloak_data

# Remove container
podman rm hexagon-keycloak
```

## References

- [Keycloak SAML Documentation](https://www.keycloak.org/docs/latest/server_admin/#saml)
- [Keycloak Container Images](https://www.keycloak.org/guides/server/containers)
- [SAML 2.0 Specification](https://docs.oasis-open.org/security/saml/v2.0/saml-core-2.0-os.pdf)
