FROM node:20-alpine AS build
WORKDIR /app

# Accept API URL as build argument
ARG API_URL=http://localhost:56808/api
ENV API_URL=$API_URL

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Replace API URL in environment file
RUN sed -i "s|http://localhost:56808/api|${API_URL}|g" src/environments/environment.ts

# Build for production
RUN npm run build

# Runtime stage with nginx
FROM nginx:alpine
COPY --from=build /app/dist/temp/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
