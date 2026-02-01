import { ApiResponse, RedisServerStats } from "@/lib/types";

export async function fetchRedisStats(
  connectionName: string,
): Promise<ApiResponse<RedisServerStats>> {
  const response = await fetch(
    `/api/redis/connections/${connectionName}/stats`,
    {
      cache: "no-store",
    },
  );

  if (!response.ok) {
    return {
      isSuccess: false,
      message: "Failed to load server stats",
      reasons: [response.statusText],
      data: {},
    };
  }

  return (await response.json()) as ApiResponse<RedisServerStats>;
}
