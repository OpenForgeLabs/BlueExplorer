import { NextRequest, NextResponse } from "next/server";
import { BaseClient } from "@/infrastructure/baseClient";
import { ServiceBusClient } from "@/infrastructure/clients/serviceBusClient";
import { RedisClient } from "@/infrastructure/clients/redisClient";
import { ApiResponse, ResourceCatalog, ResourceType } from "@/lib/types";

const DEFAULT_RESPONSE: ApiResponse<ResourceCatalog> = {
  isSuccess: true,
  message: "",
  reasons: [],
  data: { serviceBus: [], redis: [] },
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

  if (type === "all" || type === "service-bus") {
    if (!useMocks && !process.env.SERVICEBUS_API_BASE_URL) {
      response.isSuccess = false;
      response.reasons.push("Missing SERVICEBUS_API_BASE_URL.");
    } else {
      const client = new BaseClient({
        baseUrl: process.env.SERVICEBUS_API_BASE_URL ?? "http://localhost:5048",
        useMocks,
      });
      tasks.push(
        new ServiceBusClient(client).getConnections().then((result) => ({
          type: "service-bus",
          result,
        })),
      );
    }
  }

  if (type === "all" || type === "redis") {
    if (!useMocks && !process.env.REDIS_API_BASE_URL) {
      response.isSuccess = false;
      response.reasons.push("Missing REDIS_API_BASE_URL.");
    } else {
      const client = new BaseClient({
        baseUrl: process.env.REDIS_API_BASE_URL ?? "http://localhost:5060",
        useMocks,
      });
      tasks.push(
        new RedisClient(client).getConnections().then((result) => ({
          type: "redis",
          result,
        })),
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
