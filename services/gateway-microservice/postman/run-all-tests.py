#!/usr/bin/env python3
"""
Script to run all Gateway Postman tests with proper database cleanup
"""

import subprocess
import sys
import time
import os

# Change to script directory
os.chdir(os.path.dirname(os.path.abspath(__file__)))

print("=== Running Gateway Postman Tests ===")
print()

# Cleanup Privilege database before tests
print("Cleaning up Privilege database...")
subprocess.run([
    "docker", "exec", "bonus-microservice-db-prod", "psql",
    "-U", "program", "-d", "privileges",
    "-c", "TRUNCATE TABLE privilege RESTART IDENTITY CASCADE;"
], capture_output=True)

# Restart Bonus Service to clear EF cache
print("Restarting Bonus Service...")
subprocess.run(["docker", "restart", "bonus-microservice-api"], capture_output=True)
time.sleep(3)

# Run all collections
collections = ["Airport", "Booking", "Privilege", "Flight", "Ticket"]
total_passed = 0
total_failed = 0

for collection in collections:
    print(f"\nRunning {collection} tests...")
    result = subprocess.run([
        "newman", "run", f"collections/{collection}.postman_collection.json",
        "--env-var", "baseUrl=http://localhost:8080",
        "--reporters", "cli",
        "--delay-request", "300",
        "--timeout", "30000"
    ], capture_output=True, text=True)
    
    # Parse output
    for line in result.stdout.split('\n'):
        if 'assertions' in line:
            parts = line.split('│')
            if len(parts) >= 4:
                try:
                    passed = int(parts[2].strip())
                    failed = int(parts[3].strip())
                    total_passed += passed
                    total_failed += failed
                    
                    if failed == 0:
                        print(f"✅ {collection}: {passed} assertions passed")
                    else:
                        print(f"⚠️  {collection}: {passed} assertions, {failed} failed")
                except (ValueError, IndexError):
                    pass

print()
print(f"=== TOTAL: {total_passed} assertions passed, {total_failed} failed ===")

if total_failed == 0:
    print("🎉 All tests passed!")
    sys.exit(0)
else:
    print("❌ Some tests failed")
    sys.exit(1)
