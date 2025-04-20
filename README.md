# 💱 Currency Converter API

[![CI/CD - Unit Tests & Docker Build](https://github.com/ahmedamoniem/CurrencyConverterAPI/actions/workflows/ci-cd.yml/badge.svg?branch=main)](https://github.com/ahmedamoniem/CurrencyConverterAPI/actions/workflows/ci-cd.yml)

A distributed, JWT-secured .NET 8 Web API for real-time and historical currency exchange conversion using [FastEndpoints](https://fast-endpoints.com/), Redis caching, and Serilog logging to Seq.

---

## 📐 Architecture

This solution follows a clean multi-project structure:

- **Api** – Web API project with middleware, authentication, rate limiting, and endpoints.
- **Application** – Interfaces, DTOs, services, and abstraction logic.
- **Infrastructure** – HTTP clients, Redis cache, external providers (Frankfurter API).
- **Domain** – Core business models and enums.
- **Tests** – Unit + integration tests using `xUnit` and `Moq`.

---

## ⚙️ Features

-  FastEndpoints-based REST API
-  JWT authentication with role-based access (RBAC)
-  Redis distributed caching
-  Serilog logging (Seq-compatible)
-  Polly retry & circuit breaker policies
-  Swagger with versioned API support
-  Docker
-  Aspire For Telemetry 
-  GitHub Actions CI/CD (unit test + image build)

---

## 🚀 Running the App

### 🐳 Using Docker Compose

```bash
docker-compose up --build
```

This will launch:
- `currencyconverter-api` on `http://localhost:8080/swagger`
- `redis` on port `6379`
- `seq` on `http://localhost:5341`

### 🧪 Run Unit Tests

```bash
dotnet test CurrencyConverter.Tests --filter "FullyQualifiedName!~IntegrationTests"
```

---

## 🧱 Endpoints

| Endpoint                     | Method | Description                        |
|-----------------------------|--------|------------------------------------|
| `/api/rates/latest/v1`         | GET    | Get latest exchange rates          |
| `/api/rates/historical/v1`     | GET    | Get historical rates for a range   |
| `/api/rates/convert/v1`        | POST   | Convert amount between currencies  |

---

## 📦 Build Docker Image (manually)

```bash
docker build -t currencyconverter-api -f CurrencyConverter.Api/Dockerfile .
```

---

## 🔁 GitHub Actions CI/CD

- Runs on `main` branch
- Executes unit tests
- Builds and pushes Docker image to GHCR

> View the workflow [here](.github/workflows/ci-cd.yml)

---

## 🌱 Future Enhancements

- 💾 Add a persistence layer for future DB usage (e.g., PostgreSQL or SQL Server).
- ✅ Use the Result pattern to improve error handling and reduce exceptions.
- 🧾 Centralize error/exception messages in a constants or resource file.
- 🔐 Implement refresh token handling and token revocation.
- 🧪 Split unit and integration tests into separate pipelines.
- 🔄 Add fallback provider strategies for exchange rate APIs.
- 🔍 Introduce currency validation logic against ISO standards.
- 📦 Automate versioned Docker image builds using Git tags.

---

## 👥 Maintainers

- [Ahmed A Moniem](https://github.com/ahmedamoniem)

---


