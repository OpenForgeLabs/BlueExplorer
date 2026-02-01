import { BaseClient } from "@/infrastructure/baseClient";
import { ApiResponse, RedisResourceSummary } from "@/lib/types";

export class RedisClient {
  constructor(private readonly client: BaseClient) {}

  async getConnections(): Promise<ApiResponse<RedisResourceSummary[]>> {
    if (this.client.isMocked) {
      return {
        isSuccess: true,
        message: "",
        reasons: [],
        data: [
          {
            id: "redis-prod-cache",
            name: "prod-cache",
            type: "redis",
            environment: "production",
            endpoint: "prod-cache.redis.cache.windows.net",
            opsRate: "42k ops/sec",
            memoryUsage: "3.1 GB",
            keyCount: 124000,
            status: "connected",
          },
          {
            id: "redis-stg-session",
            name: "stg-session-cache",
            type: "redis",
            environment: "staging",
            endpoint: "stg-session.redis.cache.windows.net",
            opsRate: "8.4k ops/sec",
            memoryUsage: "820 MB",
            keyCount: 32000,
            status: "warning",
          },
        ],
      };
    }

    return this.client.get<RedisResourceSummary[]>("/api/connections");
  }
}
