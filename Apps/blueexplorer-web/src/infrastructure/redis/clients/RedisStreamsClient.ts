import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { ApiResponse, RedisStreamEntry } from "@/lib/types";

export class RedisStreamsClient {
  constructor(private readonly client: RedisBaseClient) {}

  getStream(
    connectionName: string,
    key: string,
    db?: number,
    start = "-",
    end = "+",
    count?: number,
  ): Promise<ApiResponse<RedisStreamEntry[]>> {
    const params = new URLSearchParams();
    if (db !== undefined && db !== null) params.set("db", db.toString());
    params.set("start", start);
    params.set("end", end);
    if (count !== undefined && count !== null) {
      params.set("count", count.toString());
    }
    const query = params.toString();
    return this.client.get<RedisStreamEntry[]>(
      `/${connectionName}/streams/${encodeURIComponent(key)}?${query}`,
    );
  }
}
