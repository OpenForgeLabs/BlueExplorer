import { NextRequest, NextResponse } from "next/server";
import { BaseClient } from "@/infrastructure/baseClient";
import { ServiceBusClient } from "@/infrastructure/clients/serviceBusClient";
import { RedisConnectionsClient } from "@/infrastructure/redis/clients/RedisConnectionsClient";
import { RedisServerClient } from "@/infrastructure/redis/clients/RedisServerClient";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import {
  ApiResponse,
  RedisConnectionInfo,
  RedisServerInfo,
  RedisResourceSummary,
  ResourceCatalog,
  ResourceType,
} from "@/lib/types";

const DEFAULT_RESPONSE: ApiResponse<ResourceCatalog> = {
  isSuccess: true,
  message: "",
  reasons: [],
  data: { serviceBus: [], redis: [] },
};

const parseKeyspaceTotal = (keyspace: Record<string, string>) => {
  let total = 0;
  Object.values(keyspace).forEach((entry) => {
    const match = entry.match(/keys=(\d+)/i);
    if (match) {
      total += Number(match[1]);
    }
  });
  return Number.isNaN(total) ? undefined : total;
};

const enrichRedisStats = (
  info?: RedisServerInfo,
): Pick<RedisResourceSummary, "opsRate" | "memoryUsage" | "keyCount"> => {
  if (!info?.sections) {
    return {
      opsRate: undefined,
      memoryUsage: undefined,
      keyCount: undefined,
    };
  }
  const stats = info.sections.Stats ?? {};
  const memory = info.sections.Memory ?? {};
  const keyspace = info.sections.Keyspace ?? {};
  const ops = stats.instantaneous_ops_per_sec
    ? Number(stats.instantaneous_ops_per_sec)
    : undefined;
  return {
    opsRate: Number.isNaN(ops) || ops === undefined ? undefined : `${ops} / sec`,
    memoryUsage: memory.used_memory_human,
    keyCount: parseKeyspaceTotal(keyspace),
  };
};

const mapRedisConnections = async (
  connections: RedisConnectionInfo[],
  client: RedisBaseClient,
): Promise<RedisResourceSummary[]> => {
  const serverClient = new RedisServerClient(client);

  return Promise.all(connections.map(async (connection) => {
    let stats: Pick<RedisResourceSummary, "opsRate" | "memoryUsage" | "keyCount"> = {
      opsRate: undefined,
      memoryUsage: undefined,
      keyCount: undefined,
    };

    if (client.isMocked) {
      stats = {
        opsRate: `${Math.floor(200 + Math.random() * 8000)} / sec`,
        memoryUsage: `${Math.floor(100 + Math.random() * 900)}MB`,
        keyCount: Math.floor(50 + Math.random() * 5000),
      };
    } else {
      const infoResult = await serverClient.getInfo(connection.name);
      if (infoResult.isSuccess) {
        stats = enrichRedisStats(infoResult.data);
      }
    }

    return {
      ...stats,
      id: `redis-${connection.name}`,
      name: connection.name,
      type: "redis",
      environment: connection.environment ?? "development",
      endpoint: connection.name,
    };
  }));
};

export async function GET(request: NextRequest) {
  const type = (request.nextUrl.searchParams.get("type") || "all") as ResourceType;
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";

  const response: ApiResponse<ResourceCatalog> = {
    ...DEFAULT_RESPONSE,
    data: { serviceBus: [], redis: [] },
  };

  const tasks: Array<
    Promise<{
      type: "service-bus" | "redis";
      result: ApiResponse<ResourceCatalog["serviceBus"] | ResourceCatalog["redis"]>;
    }>
  > = [];

  // if (type === "all" || type === "service-bus") {
  //   if (!useMocks && !process.env.SERVICEBUS_API_BASE_URL) {
  //     response.isSuccess = false;
  //     response.reasons.push("Missing SERVICEBUS_API_BASE_URL.");
  //   } else {
  //     const client = new BaseClient({
  //       baseUrl: process.env.SERVICEBUS_API_BASE_URL ?? "http://localhost:5048",
  //       basePath: "/api/servicebus",
  //       useMocks,
  //     });
  //     tasks.push(
  //       new ServiceBusClient(client).getConnections().then((result) => ({
  //         type: "service-bus",
  //         result,
  //       })),
  //     );
  //   }
  // }

  if (type === "all" || type === "redis") {
    if (!useMocks && !process.env.REDIS_API_BASE_URL) {
      response.isSuccess = false;
      response.reasons.push("Missing REDIS_API_BASE_URL.");
    } else {
      const client = new RedisBaseClient({
        baseUrl: process.env.REDIS_API_BASE_URL ?? "http://localhost:5060",
        useMocks,
      });
      tasks.push(
        new RedisConnectionsClient(client).getConnections().then(async (result) => {
          if (!result.isSuccess) {
            return {
              type: "redis",
              result: {
                ...result,
                data: [],
              } satisfies ApiResponse<RedisResourceSummary[]>,
            };
          }
          const mapped = await mapRedisConnections(result.data ?? [], client);
          return {
            type: "redis",
            result: {
              ...result,
              data: mapped,
            } satisfies ApiResponse<RedisResourceSummary[]>,
          };
        }),
      );
    }
  }

  const results = await Promise.all(tasks);

  for (const entry of results) {
    if (!entry.result.isSuccess) {
      response.isSuccess = false;
      response.message = response.message || entry.result.message;
      response.reasons.push(...(entry.result.reasons ?? []));
      continue;
    }

    if (entry.type === "service-bus") {
      response.data.serviceBus.push(
        ...((entry.result.data ?? []) as ResourceCatalog["serviceBus"]),
      );
    }

    if (entry.type === "redis") {
      response.data.redis.push(
        ...((entry.result.data ?? []) as ResourceCatalog["redis"]),
      );
    }
  }

  return NextResponse.json(response);
}
