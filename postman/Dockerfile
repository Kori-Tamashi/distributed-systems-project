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

# Copy only package files first for better caching
COPY collections/Person/ /app/collections/Person/
COPY environments/ci.environment.yaml /app/environments/

# Create output directory
RUN mkdir -p /app/test-results

# Default command - run tests with proper configuration
# Timeout: 600s total, 60s per test
CMD ["newman", "run", "/app/collections/Person/.resources/definition.yaml", \
    "--environment", "/app/environments/ci.environment.yaml", \
    "--reporters", "cli,junit,html", \
    "--reporter-junit-export", "/app/test-results/results.xml", \
    "--reporter-html-export", "/app/test-results/report.html", \
    "--delay-request", "500", \
    "--timeout", "600000", \
    "--bail"]
