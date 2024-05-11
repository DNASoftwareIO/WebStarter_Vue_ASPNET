FROM node:lts-alpine as base

WORKDIR /app

COPY Frontend/package*.json .

RUN npm install

FROM base as builder

COPY Frontend/ .

RUN npm run build

FROM node:lts-alpine

COPY --from=builder /app/dist ./

RUN npm install -g http-server

EXPOSE 8080
CMD [ "http-server", "/" ]