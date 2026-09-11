# ===========================================
# Postman Newman Docker Image for API Testing
# ===========================================
# This image is used to run Postman E2E tests in CI/CD pipeline

FROM postman/newman:alpine

# Set working directory
WORKDIR /etc/newman

# Copy collection and environment (JSON format for Newman)
COPY collections/Person.postman_collection.json /etc/newman/collection.json
COPY environments/ci.postman_environment.json /etc/newman/environment.json

# Create output directory
RUN mkdir -p /etc/newman/results

# Default command - run tests
# Timeout: 600s total
ENTRYPOINT ["sh", "-c", "newman run /etc/newman/collection.json \
  -e /etc/newman/environment.json \
  --env-var \"baseUrl=${BASE_URL:-http://person-service-e2e:8080}\" \
  --reporters cli,junit \
  --reporter-junit-export /etc/newman/results/results.xml \
  --bail"]
