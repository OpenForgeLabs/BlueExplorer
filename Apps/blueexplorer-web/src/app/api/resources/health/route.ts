import { NextRequest, NextResponse } from "next/server";
import { BaseClient } from "@/infrastructure/baseClient";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisConnectionsClient } from "@/infrastructure/redis/clients/RedisConnectionsClient";
import { ServiceBusClient } from "@/infrastructure/clients/serviceBusClient";
import { ApiResponse } from "@/lib/types";

type HealthRequest = {
  resources: Array<{ type: "service-bus" | "redis"; name: string }>;
};

type HealthResponse = {
  statuses: Record<string, boolean>;
};

const DEFAULT_SERVICEBUS_URL = "http://localhost:5048";
const DEFAULT_REDIS_URL = "http://localhost:5060";

const keyFor = (type: string, name: string) => `${type}:${name}`;

export async function POST(request: NextRequest) {
  const payload = (await request.json()) as HealthRequest;
  const resources = payload.resources ?? [];

  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";

  if (useMocks) {
    const mockStatuses = resources.reduce<Record<string, boolean>>(
      (acc, resource, index) => {
        acc[keyFor(resource.type, resource.name)] = index % 2 === 0;
        return acc;
      },
      {},
    );

    const mock: ApiResponse<HealthResponse> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: { statuses: mockStatuses },
    };

    return NextResponse.json(mock);
  }

  const serviceBusClient = new BaseClient({
    baseUrl: process.env.SERVICEBUS_API_BASE_URL ?? DEFAULT_SERVICEBUS_URL,
  });
  const redisClient = new RedisBaseClient({
    baseUrl: process.env.REDIS_API_BASE_URL ?? DEFAULT_REDIS_URL,
  });

  const serviceBus = new ServiceBusClient(serviceBusClient);
  const redis = new RedisConnectionsClient(redisClient);

  const statusEntries = await Promise.all(
    resources.map(async (resource) => {
      if (resource.type === "service-bus") {
        const result = await serviceBus.getHealth(resource.name);
        return [keyFor(resource.type, resource.name), result.data ?? false] as const;
      }

      const result = await redis.getHealth(resource.name);
      return [keyFor(resource.type, resource.name), result.data ?? false] as const;
    }),
  );

  const statuses = Object.fromEntries(statusEntries);

  const response: ApiResponse<HealthResponse> = {
    isSuccess: true,
    message: "",
    reasons: [],
    data: { statuses },
  };

  return NextResponse.json(response);
}
