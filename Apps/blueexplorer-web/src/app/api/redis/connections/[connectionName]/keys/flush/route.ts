import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisKeysClient } from "@/infrastructure/redis/clients/RedisKeysClient";
import { ApiResponse } from "@/lib/types";

const DEFAULT_BASE_URL = "http://localhost:5060";

const getClient = (request: NextRequest) => {
  const useMocks =
    request.nextUrl.searchParams.get("mock") === "true" ||
    process.env.BFF_USE_MOCKS === "true";
  const baseUrl = process.env.REDIS_API_BASE_URL ?? DEFAULT_BASE_URL;
  const client = new RedisBaseClient({ baseUrl, useMocks });
  return { client, useMocks };
};

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string }> },
) {
  const { connectionName } = await params;
  const { client, useMocks } = getClient(request);

  const dbParam = request.nextUrl.searchParams.get("db");
  const confirmName = request.nextUrl.searchParams.get("confirmName") ?? "";
  const db = dbParam ? Number(dbParam) : 0;

  if (useMocks) {
    const mock: ApiResponse<number> = {
      isSuccess: true,
      message: "Database flushed (mock).",
      reasons: [],
      data: 0,
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisKeysClient(client).flushDatabase(
    connectionName,
    db,
    confirmName,
  );
  return NextResponse.json(response);
}
