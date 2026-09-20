# ===========================================
# Postman Newman Docker Image for API Testing
# ===========================================
# Lab 02: Distributed Systems

FROM node:20-alpine

# Install Newman and Newman reporters
RUN npm install -g \
    newman \
    newman-reporter-html \
    newman-reporter-junit \
    newman-reporter-htmlextra

# Create working directory
WORKDIR /etc/newman

# Copy Postman collections and environments
COPY collections/ ./collections/
COPY environments/ ./environments/
COPY globals/ ./globals/

# Default command
CMD ["newman", "run", "collections/", "-e", "environments/local.environment.yaml", "--reporters", "cli,junit,html", "--reporter-junit-export", "results/results.xml", "--reporter-html-export", "results/index.html"]
