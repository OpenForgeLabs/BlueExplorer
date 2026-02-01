import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisKeysClient } from "@/infrastructure/redis/clients/RedisKeysClient";
import { ApiResponse, RedisKeyInfo } from "@/lib/types";

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
  { params }: { params: Promise<{ connectionName: string; key: string }> },
) {
  const { client, useMocks } = getClient(request);
  const { connectionName, key } = await params;
  const db = request.nextUrl.searchParams.get("db");

  if (useMocks) {
    const mock: ApiResponse<RedisKeyInfo> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        key,
        type: "hash",
        ttlSeconds: 342,
      },
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisKeysClient(client).getKeyInfo(
    connectionName,
    key,
    db ? Number(db) : undefined,
  );
  return NextResponse.json(response);
}
