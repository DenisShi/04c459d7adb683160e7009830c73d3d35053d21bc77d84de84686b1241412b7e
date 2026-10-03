#!/usr/bin/env bash
set -euo pipefail

psql -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  --set keycloak_db="$KEYCLOAK_DB_NAME" \
  --set keycloak_user="$KEYCLOAK_DB_USER" \
  --set keycloak_password="$KEYCLOAK_DB_PASSWORD" \
  --set app_db="$APP_DB_NAME" \
  --set app_user="$APP_DB_USER" \
  --set app_password="$APP_DB_PASSWORD" <<'SQL'
CREATE ROLE :"keycloak_user" WITH LOGIN PASSWORD :'keycloak_password';
CREATE DATABASE :"keycloak_db" WITH OWNER :"keycloak_user";
REVOKE CONNECT ON DATABASE :"keycloak_db" FROM PUBLIC;

CREATE ROLE :"app_user" WITH LOGIN PASSWORD :'app_password';
CREATE DATABASE :"app_db" WITH OWNER :"app_user";
REVOKE CONNECT ON DATABASE :"app_db" FROM PUBLIC;
SQL
