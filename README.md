Web API Challenge: Photo/Album Aggregator
Overview
A high-performance, resilient .NET 10 Web API designed to aggregate data from external sources, provide user-specific filtering, and demonstrate clean architecture principles.

Getting Started
Prerequisites
.NET 10 SDK

Git

Installation & Execution
Clone the repository.

Navigate to the root directory.

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project src/SpecKitApi
```

The API will be available at http://localhost:5000.

API Endpoints

```bash
# Health check — returns HTTP 200
curl http://localhost:5000/health

# All albums with their photos
curl http://localhost:5000/albums

# Albums filtered by user ID
curl "http://localhost:5000/albums?userId=1"
```

Response shape for `/albums`:

```json
[
  {
    "album": { "id": 1, "userId": 1, "title": "Album title" },
    "photos": [
      { "id": 1, "albumId": 1, "title": "Photo title", "imageUrl": "...", "thumbnailUrl": "..." }
    ]
  }
]
```
Technical Decisions
Architecture: Implemented a clean separation of concerns using a Class Library (Core) for domain logic and an Api project for infrastructure/controllers.

Resilience: Utilized IHttpClientFactory to manage outgoing requests, with integrated error handling to manage potential API downstream latency or failures.

SOLID Principles:

Single Responsibility: Logic for fetching and aggregating data is decoupled from the controller.

Dependency Inversion: Interfaces are used for external services, facilitating mocking for unit tests.

Testing: Unit tests implemented using xUnit to ensure business logic remains robust as the project scales.

AI Methodology Statement
AI tooling (GitHub Copilot) was utilized during the development of this project as an architectural accelerator. It was used primarily for:

Scaffolding repetitive project structures and boilerplates.

Generating unit test shells to ensure high code coverage.

Optimizing LINQ queries for data aggregation.

All generated code was manually reviewed, refactored, and integrated to ensure alignment with the requested standards and project-specific requirements.

Future Considerations (SRE Perspective)
Monitoring: Implementation of a /health endpoint to monitor downstream connectivity.

Caching: Introduction of memory caching to reduce external API dependency for non-volatile data.