import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisKeysClient } from "@/infrastructure/redis/clients/RedisKeysClient";
import { ApiResponse, RedisKeyScanResultWithInfo } from "@/lib/types";

const DEFAULT_BASE_URL = "http://localhost:5060";

const normalizeKeyType = (rawType: string | null | undefined) => {
  const value = (rawType ?? "").toLowerCase();
  if (value === "string") return "string";
  if (value === "hash") return "hash";
  if (value === "list") return "list";
  if (value === "set") return "set";
  if (value === "sortedset" || value === "zset") return "zset";
  if (value === "stream") return "stream";
  return "unknown";
};

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
    const mock: ApiResponse<RedisKeyScanResultWithInfo> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        keys: [
          { key: "session:8452", type: "hash", ttlSeconds: 342 },
          { key: "orders:stream", type: "stream", ttlSeconds: null },
          { key: "cache:product:1", type: "string", ttlSeconds: 120 },
          { key: "cache:product:2", type: "string", ttlSeconds: 95 },
          { key: "feature-flags", type: "hash", ttlSeconds: null },
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

  const response = await new RedisKeysClient(client).scanKeysWithInfo(
    connectionName,
    {
      pattern,
      pageSize: pageSize ? Number(pageSize) : undefined,
      cursor: cursor ? Number(cursor) : undefined,
      db: db ? Number(db) : undefined,
    },
  );
  if (!response.isSuccess || !response.data) {
    return NextResponse.json(response);
  }

  return NextResponse.json({
    ...response,
    data: {
      ...response.data,
      keys: response.data.keys.map((info) => ({
        ...info,
        type: normalizeKeyType(info.type),
      })),
    },
  });
}
