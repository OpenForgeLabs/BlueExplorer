import { ApiResponse, RedisKeyType, RedisKeyValue } from "@/lib/types";

export async function fetchRedisKeyValue(
  connectionName: string,
  key: string,
  type: RedisKeyType,
  db?: number,
): Promise<ApiResponse<RedisKeyValue>> {
  const params = new URLSearchParams();
  params.set("type", type);
  if (db !== undefined && db !== null) params.set("db", db.toString());
  const query = params.toString();
  const response = await fetch(
    `/api/redis/connections/${connectionName}/keys/${encodeURIComponent(
      key,
    )}/value?${query}`,
    { cache: "no-store" },
  );

  if (!response.ok) {
    return {
      isSuccess: false,
      message: "Failed to load key value",
      reasons: [response.statusText],
      data: { type: "unknown", value: null },
    };
  }

  return (await response.json()) as ApiResponse<RedisKeyValue>;
}

export async function updateRedisKeyValue(
  connectionName: string,
  key: string,
  type: RedisKeyType,
  value: unknown,
  db?: number,
  expirySeconds?: number,
): Promise<ApiResponse<boolean>> {
  const params = new URLSearchParams();
  if (db !== undefined && db !== null) params.set("db", db.toString());
  const query = params.toString();
  const response = await fetch(
    `/api/redis/connections/${connectionName}/keys/${encodeURIComponent(
      key,
    )}/value${query ? `?${query}` : ""}`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ type, value, expirySeconds }),
    },
  );

  if (!response.ok) {
    return {
      isSuccess: false,
      message: "Failed to update key",
      reasons: [response.statusText],
      data: false,
    };
  }

  return (await response.json()) as ApiResponse<boolean>;
}
