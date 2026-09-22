#!/bin/bash

# Script to run all Gateway Postman tests with proper database cleanup

echo "=== Running Gateway Postman Tests ==="
echo ""

# Cleanup Privilege database before tests
echo "Cleaning up Privilege database..."
docker exec bonus-microservice-db-prod psql -U program -d privileges -c "TRUNCATE TABLE privilege RESTART IDENTITY CASCADE;" > /dev/null 2>&1

# Restart Bonus Service to clear EF cache
echo "Restarting Bonus Service..."
docker restart bonus-microservice-api > /dev/null 2>&1
sleep 3

# Run all collections
collections=("Airport" "Booking" "Privilege" "Flight" "Ticket")
total_passed=0
total_failed=0

for collection in "${collections[@]}"; do
    echo ""
    echo "Running ${collection} tests..."
    result=$(newman run "collections/${collection}.postman_collection.json" \
        --env-var baseUrl=http://localhost:8080 \
        --reporters cli \
        --delay-request 300 \
        --timeout 30000 2>&1)
    
    passed=$(echo "$result" | grep "assertions" | sed 's/.*│\s*\([0-9]*\)\s*│\s*\([0-9]*\)\s*│.*/\1/')
    failed=$(echo "$result" | grep "assertions" | sed 's/.*│\s*\([0-9]*\)\s*│\s*\([0-9]*\)\s*│.*/\2/')
    
    if [ -z "$passed" ]; then
        passed=0
        failed=0
    fi
    
    total_passed=$((total_passed + passed))
    total_failed=$((total_failed + failed))
    
    if [ "$failed" = "0" ]; then
        echo "✅ ${collection}: ${passed} assertions passed"
    else
        echo "⚠️  ${collection}: ${passed} assertions, ${failed} failed"
    fi
done

echo ""
echo "=== TOTAL: ${total_passed} assertions passed, ${total_failed} failed ==="

if [ "$total_failed" = "0" ]; then
    echo "🎉 All tests passed!"
    exit 0
else
    echo "❌ Some tests failed"
    exit 1
fi
