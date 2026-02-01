import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisKeysClient } from "@/infrastructure/redis/clients/RedisKeysClient";
import { ApiResponse, RedisKeyScanResult } from "@/lib/types";

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
  { params }: { params: { connectionName: string } },
) {
  const { client, useMocks } = getClient(request);

  if (useMocks) {
    const mock: ApiResponse<RedisKeyScanResult> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        keys: [
          "session:8452",
          "orders:stream",
          "cache:product:1",
          "cache:product:2",
          "feature-flags",
        ],
        cursor: 0,
      },
    };
    return NextResponse.json(mock);
  }

  const pattern = request.nextUrl.searchParams.get("pattern") ?? undefined;
  const pageSize = request.nextUrl.searchParams.get("pageSize");
  const cursor = request.nextUrl.searchParams.get("cursor");
  const db = request.nextUrl.searchParams.get("db");

  const response = await new RedisKeysClient(client).scanKeys(
    params.connectionName,
    {
      pattern,
      pageSize: pageSize ? Number(pageSize) : undefined,
      cursor: cursor ? Number(cursor) : undefined,
      db: db ? Number(db) : undefined,
    },
  );
  return NextResponse.json(response);
}
