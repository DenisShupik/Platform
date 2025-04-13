FROM node:slim

RUN corepack enable && corepack prepare pnpm@latest --activate

WORKDIR /app

COPY package.json ./

RUN pnpm install

COPY . .

CMD ["pnpm", "dev"]