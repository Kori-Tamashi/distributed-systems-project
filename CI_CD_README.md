# CI/CD Pipeline - Lab 02: Distributed Systems

## 📋 Overview

This project uses GitHub Actions for continuous integration and continuous deployment (CI/CD) across all microservices:

- **Gateway Microservice** (port 8080)
- **Flight Microservice** (port 8060)
- **Ticket Microservice** (port 8070)
- **Bonus Microservice** (port 8050)

## 🔄 Pipeline Stages

### 1. **CodeQL Security Analysis** (Parallel)
- Static code analysis for security vulnerabilities
- Runs on all pushes and pull requests
- Languages: C#

### 2. **Build** (Parallel for all services)
- Restore NuGet dependencies
- Build all projects in Release configuration
- Analyze dependencies for license compliance
- Upload build artifacts

### 3. **Unit Tests** (Parallel for all services)
- Run all unit tests with coverage collection
- Filter: `FullyQualifiedName~Unit`
- Upload test results (TRX format)
- Generate test summaries

### 4. **Integration Tests** (Parallel for all services)
- Spin up PostgreSQL 14 container
- Run integration tests with real database
- Filter: `FullyQualifiedName~Integration`
- Upload test results with coverage

### 5. **Docker Build Test** (Parallel for all services)
- Build Docker images for each service
- Test container startup and health
- Security scan with Trivy
- Check image size

### 6. **API Tests** (Parallel for all services)
- Build Docker images
- Start containers with PostgreSQL
- Run Postman Newman tests
- Upload JUnit test results

### 7. **CD - Docker Push** (On success)
- Triggered after successful CI
- Push images to Docker Hub
- Tag with SHA, latest, and semantic versions

## 📁 Workflow Files

```
.github/workflows/
├── ci.yml           # Build and Test pipeline
├── cd.yml           # Docker push pipeline
└── api-tests.yml    # Postman API tests
```

## 🧪 Test Coverage

Each microservice has:
- **Unit Tests**: Business logic, converters, gateways
- **Integration Tests**: Database operations, HTTP clients
- **API Tests**: End-to-end testing with Postman/Newman

## 🐳 Docker Images

Images are pushed to Docker Hub with the following pattern:

```
{DOCKER_USERNAME}/gateway-microservice:{tag}
{DOCKER_USERNAME}/flight-microservice:{tag}
{DOCKER_USERNAME}/ticket-microservice:{tag}
{DOCKER_USERNAME}/bonus-microservice:{tag}
```

Tags:
- `latest` - on main branch
- `{sha}` - commit SHA
- `{version}` - semantic version

## 🔧 Environment Variables

```yaml
DOTNET_VERSION: '10.0.x'
DOCKER_USERNAME: ${{ secrets.DOCKER_USERNAME }}
DOCKER_PASSWORD: ${{ secrets.DOCKER_PASSWORD }}
```

## 📊 Test Results

Test results are published to:
- GitHub Artifacts (TRX, coverage)
- GitHub PR/MR checks (Test Reporter)
- Test summaries in workflow output

## 🚀 Triggers

### CI Pipeline
- Push to `main`, `develop`
- Pull requests to `main`

### CD Pipeline
- Workflow run completion (after successful CI)
- Manual dispatch (`workflow_dispatch`)

### API Tests
- Workflow run completion (after successful CI)
- Pull requests to `main`
- Manual dispatch

## 🛠️ Manual Execution

```bash
# Trigger CI manually
gh workflow run "CI - Build and Test All Microservices"

# Trigger CD manually
gh workflow run "CD - Push Docker Images"

# Trigger API tests manually
gh workflow run "API Tests - All Microservices"
```

## 📝 Concurrency Control

Duplicate runs are cancelled automatically:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

## 🔐 Secrets Required

- `DOCKER_USERNAME` - Docker Hub username
- `DOCKER_PASSWORD` - Docker Hub access token

## ✅ Pipeline Status

| Stage | Status |
|-------|--------|
| CodeQL | ✅ |
| Build | ✅ |
| Unit Tests | ✅ |
| Integration Tests | ✅ |
| Docker Build | ✅ |
| API Tests | ✅ |
| CD Push | ✅ |

## 📚 References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Newman CLI](https://www.postman.com/product/api-testing/newman/)
- [CodeQL](https://codeql.github.com/)
- [Trivy Security Scanner](https://aquasecurity.github.io/trivy/)
