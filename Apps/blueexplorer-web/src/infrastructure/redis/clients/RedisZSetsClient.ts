import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { ApiResponse, RedisZSetEntry } from "@/lib/types";

export class RedisZSetsClient {
  constructor(private readonly client: RedisBaseClient) {}

  getZSet(
    connectionName: string,
    key: string,
    db?: number,
    start = 0,
    stop = -1,
  ): Promise<ApiResponse<RedisZSetEntry[]>> {
    const params = new URLSearchParams();
    if (db !== undefined && db !== null) params.set("db", db.toString());
    params.set("start", start.toString());
    params.set("stop", stop.toString());
    params.set("withScores", "true");
    const query = params.toString();
    return this.client.get<RedisZSetEntry[]>(
      `/${connectionName}/zsets/${encodeURIComponent(key)}?${query}`,
    );
  }
}
