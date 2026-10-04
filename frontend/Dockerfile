FROM --platform=$BUILDPLATFORM node:24.21.0-alpine3.24 AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.30.5-alpine3.24 AS runtime
ENV KEYCLOAK_URL=http://localhost:8080 \
    KEYCLOAK_REALM=phonebook \
    KEYCLOAK_CLIENT_ID=phonebook-spa \
    API_UPSTREAM=http://backend:8080 \
    NGINX_RESOLVER=127.0.0.11
COPY nginx/security-headers.conf /etc/nginx/snippets/security-headers.conf
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/phonebook/browser /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1
