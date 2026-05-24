# Quickstart: Albums API Endpoints

**Feature**: `004-albums-api-endpoints`

This guide shows how to run the API locally and exercise both endpoints with `curl`.

---

## Prerequisites

- .NET 10 SDK installed (`dotnet --version` should show `10.x.x`)
- Internet access (or a local mock) for the JSONPlaceholder upstream at `https://jsonplaceholder.typicode.com`

---

## Run the API

From the repository root:

```powershell
dotnet run --project src/SpecKitApi/SpecKitApi.csproj
```

The API starts on `http://localhost:5000` by default (configured in `launchSettings.json`). You should see output similar to:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

---

## Health Check

Confirm the API is running:

```bash
curl -i http://localhost:5000/health
```

Expected response:

```
HTTP/1.1 200 OK
Content-Length: 0
```

---

## Retrieve All Albums

Fetch the full list of albums with their photos:

```bash
curl -s http://localhost:5000/albums | head -c 500
```

Expected (truncated):

```json
[
  {
    "album": {
      "id": 1,
      "userId": 1,
      "title": "quidem molestiae enim"
    },
    "photos": [
      {
        "id": 1,
        "albumId": 1,
        "title": "accusamus beatae ad facilis cum similique qui sunt",
        "imageUrl": "https://via.placeholder.com/600/92c952",
        "thumbnailUrl": "https://via.placeholder.com/150/92c952"
      }
    ]
  },
  ...
]
```

> The full response is ~100 albums × ~50 photos each. Pipe to `| python -m json.tool` or `| jq '.[0]'` for readable output.

---

## Retrieve Albums for a Specific User

Filter to albums owned by user 1:

```bash
curl -s "http://localhost:5000/albums?userId=1" | jq 'length'
```

Expected: `10` (user 1 owns 10 albums in JSONPlaceholder).

---

## Invalid `userId` — 400 Response

Pass a non-integer to see the structured error:

```bash
curl -i "http://localhost:5000/albums?userId=abc"
```

Expected:

```
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8

{"message":"userId must be a positive integer.","code":"INVALID_PARAMETER","correlationId":"<generated-id>"}
```

---

## Invalid `userId` — Zero or Negative

```bash
curl -i "http://localhost:5000/albums?userId=0"
curl -i "http://localhost:5000/albums?userId=-5"
```

Both return `400` with the same `ErrorResponse` body.

---

## Correlation ID

Pass your own correlation ID header to trace a request through the logs:

```bash
curl -i -H "X-Correlation-ID: my-trace-abc123" http://localhost:5000/albums?userId=1
```

The response will echo the header:

```
X-Correlation-ID: my-trace-abc123
```

And the server logs will include `"CorrelationId": "my-trace-abc123"` on every log entry for that request.

---

## Run Tests

Run all tests (unit + integration) from the repository root:

```powershell
dotnet test
```

Expected summary (after full implementation):

```
Passed!  - Failed: 0, Passed: 31+, Skipped: 0, Total: 31+
```

Run only unit tests (no web host):

```powershell
dotnet test --filter "FullyQualifiedName!~Integration"
```

Run only integration tests:

```powershell
dotnet test --filter "FullyQualifiedName~Integration"
```
