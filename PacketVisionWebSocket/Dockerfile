# Base image
FROM node:20

# Create app directory
WORKDIR /app

# Copy package.json and install dependencies
COPY package*.json ./
RUN npm install

# Copy the rest of the code
COPY . .

# Expose WebSocket port (default 8082, change as needed!)
EXPOSE 3000 3001

# Start the WebSocket server
CMD [ "node", "websocket.js" ]