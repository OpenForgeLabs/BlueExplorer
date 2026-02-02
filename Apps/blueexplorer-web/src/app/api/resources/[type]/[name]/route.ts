import { NextRequest, NextResponse } from "next/server";
import { BaseClient } from "@/infrastructure/baseClient";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisConnectionsClient } from "@/infrastructure/redis/clients/RedisConnectionsClient";
import { ServiceBusClient } from "@/infrastructure/clients/serviceBusClient";
import { ApiResponse } from "@/lib/types";

const DEFAULT_REDIS_URL = "http://localhost:5095";
const DEFAULT_SERVICEBUS_URL = "http://localhost:5048";

type RouteParams = {
  type: string;
  name: string;
};

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<RouteParams> },
) {
  const { type, name } = await params;
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";

  if (useMocks) {
    const mock: ApiResponse<void> = {
      isSuccess: true,
      message: "Resource deleted (mock).",
      reasons: [],
      data: undefined as void,
    };
    return NextResponse.json(mock);
  }

  if (type === "redis") {
    const baseUrl = process.env.REDIS_API_BASE_URL ?? DEFAULT_REDIS_URL;
    const client = new RedisBaseClient({ baseUrl, useMocks: false });
    const response = await new RedisConnectionsClient(client).deleteConnection(
      decodeURIComponent(name),
    );
    return NextResponse.json(response);
  }

  const baseUrl = process.env.SERVICEBUS_API_BASE_URL ?? DEFAULT_SERVICEBUS_URL;
  const client = new BaseClient({ baseUrl, useMocks: false });
  const response = await new ServiceBusClient(client).deleteConnection(
    decodeURIComponent(name),
  );
  return NextResponse.json(response);
}
