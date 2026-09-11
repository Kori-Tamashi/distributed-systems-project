# ===========================================
# Postman Newman Docker Image for API Testing
# ===========================================
# This image is used to run Postman E2E tests in CI/CD pipeline

FROM node:20-alpine

# Install dependencies
RUN apk add --no-cache \
    curl \
    jq

# Install Newman and reporters
RUN npm install -g \
    newman \
    newman-reporter-html \
    newman-reporter-junit \
    newman-reporter-htmlextra

# Set working directory
WORKDIR /app

# Copy Postman collection and environments
COPY collections/Person/ /app/collections/Person/
COPY environments/local.environment.yaml /app/environments/

# Default command - run tests
CMD ["newman", "run", "/app/collections/Person/.resources/definition.yaml", "--environment", "/app/environments/local.environment.yaml", "--reporters", "cli,junit", "--reporter-junit-export", "/app/test-results/results.xml"]
