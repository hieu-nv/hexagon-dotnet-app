#!/bin/bash

# dev → development
# stg → staging
# prd → production

podman run --name auth-dev \
  -p 127.0.0.1:8080:8080 \
  -e KC_BOOTSTRAP_ADMIN_USERNAME=admin \
  -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:26.5.4 start-dev
  

