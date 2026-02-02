import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisServerClient } from "@/infrastructure/redis/clients/RedisServerClient";
import { ApiResponse, RedisServerStats } from "@/lib/types";

const DEFAULT_BASE_URL = "http://localhost:5060";

const getClient = (request: NextRequest) => {
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";
  const baseUrl = process.env.REDIS_API_BASE_URL ?? DEFAULT_BASE_URL;
  const client = new RedisBaseClient({ baseUrl, useMocks });
  return { client, useMocks };
};

const toNumber = (value?: string) => {
  if (!value) return undefined;
  const parsed = Number(value);
  return Number.isNaN(parsed) ? undefined : parsed;
};

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string }> },
) {
  const { connectionName } = await params;
  const { client, useMocks } = getClient(request);

  if (useMocks) {
    const mock: ApiResponse<RedisServerStats> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        version: "7.2.4",
        uptimeSeconds: 93211,
        connectedClients: 18,
        opsPerSec: 42800,
        usedMemoryHuman: "3.1G",
        keyspace: "db0:keys=124000,expires=31000,avg_ttl=905000",
      },
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisServerClient(client).getInfo(connectionName);

  if (!response.isSuccess || !response.data) {
    return NextResponse.json(response as ApiResponse<RedisServerStats>);
  }

  const sections = response.data.sections ?? {};
  const server = sections.Server ?? {};
  const stats = sections.Stats ?? {};
  const clients = sections.Clients ?? {};
  const memory = sections.Memory ?? {};
  const keyspace = sections.Keyspace ?? {};

  const statsResponse: ApiResponse<RedisServerStats> = {
    isSuccess: true,
    message: response.message,
    reasons: response.reasons,
    data: {
      version: server.redis_version,
      uptimeSeconds: toNumber(server.uptime_in_seconds),
      connectedClients: toNumber(clients.connected_clients),
      opsPerSec: toNumber(stats.instantaneous_ops_per_sec),
      usedMemoryHuman: memory.used_memory_human,
      keyspace: Object.values(keyspace).join(" | "),
    },
  };

  return NextResponse.json(statsResponse);
}
