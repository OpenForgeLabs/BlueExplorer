import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisConnectionsClient } from "@/infrastructure/redis/clients/RedisConnectionsClient";
import {
  ApiResponse,
  RedisConnectionUpsertRequest,
} from "@/lib/types";

const DEFAULT_BASE_URL = "http://localhost:5060";

const getClient = (request: NextRequest) => {
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";
  const baseUrl = process.env.REDIS_API_BASE_URL ?? DEFAULT_BASE_URL;
  const client = new RedisBaseClient({ baseUrl, useMocks });
  return { client, useMocks };
};

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string }> },
) {
  const { connectionName } = await params;
  const { client, useMocks } = getClient(request);

  if (useMocks) {
    const mock: ApiResponse<RedisConnectionUpsertRequest> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        name: connectionName,
        connectionString: "",
        host: "localhost",
        port: 6379,
        password: "",
        useTls: false,
        database: 0,
      },
    };
    return NextResponse.json(mock);
  }

  const response =
    await new RedisConnectionsClient(client).getConnection(connectionName);
  return NextResponse.json(response);
}

export async function PUT(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string }> },
) {
  const { connectionName } = await params;
  const { client, useMocks } = getClient(request);
  const body = (await request.json()) as RedisConnectionUpsertRequest;

  if (useMocks) {
    const mock: ApiResponse<void> = {
      isSuccess: true,
      message: "Connection updated (mock).",
      reasons: [],
      data: undefined as void,
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisConnectionsClient(client).updateConnection(
    connectionName,
    body,
  );
  return NextResponse.json(response);
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string }> },
) {
  const { connectionName } = await params;
  const { client, useMocks } = getClient(request);

  if (useMocks) {
    const mock: ApiResponse<void> = {
      isSuccess: true,
      message: "Connection deleted (mock).",
      reasons: [],
      data: undefined as void,
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisConnectionsClient(client).deleteConnection(
    connectionName,
  );
  return NextResponse.json(response);
}
