import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { ApiResponse, RedisKeyInfo, RedisKeyScanResult } from "@/lib/types";

export type RedisKeyScanParams = {
  pattern?: string;
  pageSize?: number;
  cursor?: number;
  db?: number;
};

export class RedisKeysClient {
  constructor(private readonly client: RedisBaseClient) {}

  scanKeys(
    connectionName: string,
    params: RedisKeyScanParams,
  ): Promise<ApiResponse<RedisKeyScanResult>> {
    const query = new URLSearchParams();
    if (params.pattern) query.set("pattern", params.pattern);
    if (params.pageSize) query.set("pageSize", params.pageSize.toString());
    if (params.cursor) query.set("cursor", params.cursor.toString());
    if (params.db !== undefined && params.db !== null) {
      query.set("db", params.db.toString());
    }
    const suffix = query.toString() ? `?${query.toString()}` : "";
    return this.client.get<RedisKeyScanResult>(
      `/${connectionName}/keys${suffix}`,
    );
  }

  getKeyInfo(
    connectionName: string,
    key: string,
    db?: number,
  ): Promise<ApiResponse<RedisKeyInfo>> {
    const query =
      db !== undefined && db !== null ? `?db=${db.toString()}` : "";
    return this.client.get<RedisKeyInfo>(
      `/${connectionName}/keys/${encodeURIComponent(key)}/info${query}`,
    );
  }
}
