FROM node:slim AS base
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable
COPY . /app
WORKDIR /app

FROM base AS prod-deps
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --prod --frozen-lockfile

FROM base AS build
ENV PUBLIC_KEYCLOAK_URL=https://forum-node.ru/auth
ENV PUBLIC_KEYCLOAK_REALM=forum
ENV PUBLIC_KEYCLOAK_CLIENT_ID=forum-user
ENV PUBLIC_AVATAR_URL=https://forum-node.ru/files/avatars
ENV PUBLIC_CSR_API_URL=https://forum-node.ru
ENV PUBLIC_APP_NAME=FORUM
ENV PUBLIC_SSR_API_URL=http://api-gateway:8000
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

FROM base
COPY --from=build /app/build /app/build

CMD [ "node", "build" ]