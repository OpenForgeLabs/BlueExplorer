# BlueExplorer

BlueExplorer is an Azure-first infrastructure explorer. It provides a web UI plus APIs to manage resources like Azure Service Bus and Redis.

## Docker (GHCR)

Images are published to GitHub Container Registry:

- `ghcr.io/openforgelabs/blueexplorer-web`
- `ghcr.io/openforgelabs/blueexplorer-servicebus`
- `ghcr.io/openforgelabs/blueexplorer-redis`

### Run with Docker

```bash
docker run -p 3000:3000 \
  -e SERVICEBUS_API_BASE_URL=http://localhost:5048 \
  -e REDIS_API_BASE_URL=http://localhost:5095 \
  ghcr.io/openforgelabs/blueexplorer-web:latest
```

### Example docker-compose

```yaml
services:
  servicebus:
    image: ghcr.io/openforgelabs/blueexplorer-servicebus:latest
    ports:
      - "5048:8080"

  redis-api:
    image: ghcr.io/openforgelabs/blueexplorer-redis:latest
    ports:
      - "5095:8080"

  web:
    image: ghcr.io/openforgelabs/blueexplorer-web:latest
    ports:
      - "3000:3000"
    environment:
      SERVICEBUS_API_BASE_URL: http://servicebus:8080
      REDIS_API_BASE_URL: http://redis-api:8080
      BFF_USE_MOCKS: "false"
    depends_on:
      - servicebus
      - redis-api
```

Registry namespace is set to `openforgelabs`.

## Release workflow

Publishing happens via `.github/workflows/docker.yml` on:

- push to `master` (tags `latest`)
- tags like `vX.Y.Z` (tags `vX.Y.Z`)

## Branch protection checklist

Recommended settings for `master`:

- Require pull request reviews (1+ approval)
- Require status checks (CI) to pass
- Require linear history (optional)
- Disallow force pushes
- Restrict direct pushes to `master`
