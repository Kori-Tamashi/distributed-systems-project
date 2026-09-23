# ===========================================
# Postman Newman Docker Image for API Testing
# ===========================================

FROM postman/newman:alpine

WORKDIR /etc/newman

COPY collections/Flight.postman_collection.json /etc/newman/collection.json
COPY environments/local.postman_environment.json /etc/newman/environment.json

RUN mkdir -p /etc/newman/results

ENTRYPOINT ["sh", "-c", "newman run /etc/newman/collection.json \
  -e /etc/newman/environment.json \
  --env-var \"baseUrl=${BASE_URL:-http://localhost:8060}\" \
  --reporters cli,junit \
  --reporter-junit-export /etc/newman/results/results.xml \
  --delay-request 100 \
  --timeout 30000 \
  --bail false"]
