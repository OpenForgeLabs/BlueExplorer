import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisConnectionsClient } from "@/infrastructure/redis/clients/RedisConnectionsClient";
import {
  ApiResponse,
  RedisConnectionInfo,
  RedisConnectionUpsertRequest,
} from "@/lib/types";

const DEFAULT_BASE_URL = "http://localhost:5095";

const getClient = (request: NextRequest) => {
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";
  const baseUrl = process.env.REDIS_API_BASE_URL ?? DEFAULT_BASE_URL;
  const client = new RedisBaseClient({ baseUrl, useMocks });
  return { client, useMocks };
};

export async function GET(request: NextRequest) {
  const { client, useMocks } = getClient(request);

  if (useMocks) {
    const mock: ApiResponse<RedisConnectionInfo[]> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: [
        {
          name: "prod-cache",
          useTls: true,
          database: 0,
          isEditable: true,
          source: "local",
          environment: "production",
        },
        {
          name: "stg-session-cache",
          useTls: false,
          database: 1,
          isEditable: true,
          source: "local",
          environment: "staging",
        },
      ],
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisConnectionsClient(client).getConnections();
  return NextResponse.json(response);
}

export async function POST(request: NextRequest) {
  const { client, useMocks } = getClient(request);
  const body = (await request.json()) as RedisConnectionUpsertRequest;

  if (useMocks) {
    const mock: ApiResponse<void> = {
      isSuccess: true,
      message: "Connection saved (mock).",
      reasons: [],
      data: undefined as void,
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisConnectionsClient(client).createConnection(body);
  return NextResponse.json(response);
}
