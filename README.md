# BlueExplorer

BlueExplorer is an Azure-first infrastructure explorer that gives developers a single, focused UI to manage operational resources. It pairs a modular web app with resource-specific APIs (Redis UI today; Service Bus API available, UI coming soon) so you can extend or run only what you need.

## ✨ Highlights

- Modular resources (Redis UI today; Service Bus API, UI coming soon)
- Local-first, developer-friendly UX
- Docker-ready deployment
- BFF layer to keep the UI stable

## 🧱 Images (GHCR)

- `ghcr.io/openforgelabs/blueexplorer-web`
- `ghcr.io/openforgelabs/blueexplorer-servicebus`
- `ghcr.io/openforgelabs/blueexplorer-redis`

## 🚀 Run with Docker

```bash
docker run -p 3000:3000 \
  -e SERVICEBUS_API_BASE_URL=http://localhost:5048 \
  -e REDIS_API_BASE_URL=http://localhost:5095 \
  ghcr.io/openforgelabs/blueexplorer-web:latest
```

### docker-compose example

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

## 🧩 Module-first Architecture

BlueExplorer is structured so each infrastructure resource can be enabled or shipped independently:

- A resource API per module (Service Bus, Redis, Blob later)
- A web UI that discovers available modules
- BFF orchestration to keep UI contracts consistent

## 🗺️ Roadmap (Short Term)

- Redis key browser UX improvements
- Service Bus: refine the API and build the UI
- Blob Storage module
- Packaging without Docker (optional)

## 💰 Support BlueExplorer

If BlueExplorer saves you time, please consider supporting the project:

[![PayPal](https://img.shields.io/badge/Donate%20via%20PayPal-00457C?style=for-the-badge&logo=paypal&logoColor=white)](https://paypal.me/JuanTellezRojas)

## 📦 Release Workflow

Docker images are published **only on releases** via `.github/workflows/docker.yml`:

- Create a GitHub Release with a `vX.Y.Z` tag
- CI builds multi-arch images
- A `release-manifest.json` is attached to the release
