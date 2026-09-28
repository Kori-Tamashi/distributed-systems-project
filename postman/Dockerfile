# ===========================================
# Postman Newman Runner - Instructor Tests
# ===========================================

FROM postman/newman:latest

WORKDIR /etc/newman

# Copy instructor test collection and environment
COPY collection.json /etc/newman/collection.json
COPY environment.json /etc/newman/environment.json

# Copy fault tolerance test collection
COPY fault-tolerance-collection.json /etc/newman/fault-tolerance-collection.json

# Default command - run instructor tests
CMD ["run", "/etc/newman/collection.json", "-e", "/etc/newman/environment.json", "--reporters", "cli,junit", "--reporter-junit-export", "/etc/newman/results/results.xml"]
