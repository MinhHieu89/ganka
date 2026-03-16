FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG VITE_API_URL=""
ENV VITE_API_URL=${VITE_API_URL}
RUN npm run build

# Serve SPA static files
FROM node:22-alpine AS runtime
WORKDIR /app

RUN npm install -g serve@latest

COPY --from=build /app/dist/client ./public
RUN mv public/_shell.html public/index.html

EXPOSE 3000

CMD ["serve", "public", "-l", "3000", "-s"]
