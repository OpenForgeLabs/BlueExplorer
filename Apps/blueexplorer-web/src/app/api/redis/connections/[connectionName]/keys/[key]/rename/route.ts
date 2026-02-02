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
  { params }: { params: Promise<{ connectionName: string; key: string }> },
) {
  const { client, useMocks } = getClient(request);
  const { connectionName, key } = await params;
  const db = request.nextUrl.searchParams.get("db");
  const body = (await request.json()) as { newKey?: string };

  if (useMocks) {
    const mock: ApiResponse<boolean> = {
      isSuccess: true,
      message: "Renamed (mock).",
      reasons: [],
      data: true,
    };
    return NextResponse.json(mock);
  }

  if (!body?.newKey) {
    const response: ApiResponse<boolean> = {
      isSuccess: false,
      message: "New key is required",
      reasons: ["newKey is missing"],
      data: false,
    };
    return NextResponse.json(response, { status: 400 });
  }

  const response = await new RedisKeysClient(client).renameKey(
    connectionName,
    key,
    body.newKey,
    db ? Number(db) : undefined,
  );
  return NextResponse.json(response);
}
