import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { ApiResponse } from "@/lib/types";

export class RedisListsClient {
  constructor(private readonly client: RedisBaseClient) {}

  getList(
    connectionName: string,
    key: string,
    db?: number,
    start = 0,
    stop = -1,
  ): Promise<ApiResponse<string[]>> {
    const params = new URLSearchParams();
    if (db !== undefined && db !== null) params.set("db", db.toString());
    params.set("start", start.toString());
    params.set("stop", stop.toString());
    const query = params.toString();
    return this.client.get<string[]>(
      `/${connectionName}/lists/${encodeURIComponent(key)}?${query}`,
    );
  }
}
