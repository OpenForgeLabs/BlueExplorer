import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisHashesClient } from "@/infrastructure/redis/clients/RedisHashesClient";
import { RedisListsClient } from "@/infrastructure/redis/clients/RedisListsClient";
import { RedisSetsClient } from "@/infrastructure/redis/clients/RedisSetsClient";
import { RedisStreamsClient } from "@/infrastructure/redis/clients/RedisStreamsClient";
import { RedisStringsClient } from "@/infrastructure/redis/clients/RedisStringsClient";
import { RedisZSetsClient } from "@/infrastructure/redis/clients/RedisZSetsClient";
import { ApiResponse, RedisKeyType, RedisKeyValue } from "@/lib/types";

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
  const type = (request.nextUrl.searchParams.get("type") ??
    "unknown") as RedisKeyType;
  const db = request.nextUrl.searchParams.get("db");

  if (useMocks) {
    const mock: ApiResponse<RedisKeyValue> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: {
        type: "hash",
        value: {
          last_login: "2024-11-25T14:22:01Z",
          ip_address: "192.168.1.105",
          status: "active",
        },
      },
    };
    return NextResponse.json(mock);
  }

  const dbValue = db ? Number(db) : undefined;
  const strings = new RedisStringsClient(client);
  const hashes = new RedisHashesClient(client);
  const lists = new RedisListsClient(client);
  const sets = new RedisSetsClient(client);
  const zsets = new RedisZSetsClient(client);
  const streams = new RedisStreamsClient(client);

  let response: ApiResponse<RedisKeyValue>;

  switch (type) {
    case "string": {
      const result = await strings.getString(
        connectionName,
        key,
        dbValue,
      );
      response = {
        ...result,
        data: { type: "string", value: result.data ?? null },
      };
      break;
    }
    case "hash": {
      const result = await hashes.getHash(
        connectionName,
        key,
        dbValue,
      );
      response = {
        ...result,
        data: { type: "hash", value: result.data ?? {} },
      };
      break;
    }
    case "list": {
      const result = await lists.getList(
        connectionName,
        key,
        dbValue,
      );
      response = {
        ...result,
        data: { type: "list", value: result.data ?? [] },
      };
      break;
    }
    case "set": {
      const result = await sets.getSet(
        connectionName,
        key,
        dbValue,
      );
      response = {
        ...result,
        data: { type: "set", value: result.data ?? [] },
      };
      break;
    }
    case "zset": {
      const result = await zsets.getZSet(
        connectionName,
        key,
        dbValue,
      );
      response = {
        ...result,
        data: { type: "zset", value: result.data ?? [] },
      };
      break;
    }
    case "stream": {
      const result = await streams.getStream(
        connectionName,
        key,
        dbValue,
        "-",
        "+",
        200,
      );
      response = {
        ...result,
        data: { type: "stream", value: result.data ?? [] },
      };
      break;
    }
    default: {
      response = {
        isSuccess: false,
        message: "Unsupported key type",
        reasons: [],
        data: { type: "unknown", value: null },
      };
      break;
    }
  }

  return NextResponse.json(response);
}

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string; key: string }> },
) {
  const { client, useMocks } = getClient(request);
  const { connectionName, key } = await params;
  const db = request.nextUrl.searchParams.get("db");
  const body = (await request.json()) as {
    type: RedisKeyType;
    value: unknown;
    expirySeconds?: number;
  };

  if (useMocks) {
    const mock: ApiResponse<boolean> = {
      isSuccess: true,
      message: "Updated (mock).",
      reasons: [],
      data: true,
    };
    return NextResponse.json(mock);
  }

  const dbValue = db ? Number(db) : undefined;
  const strings = new RedisStringsClient(client);
  const hashes = new RedisHashesClient(client);

  if (body.type === "string") {
    const response = await strings.setString(
      connectionName,
      key,
      String(body.value ?? ""),
      body.expirySeconds,
      dbValue,
    );
    return NextResponse.json(response);
  }

  if (body.type === "hash") {
    const entries = (body.value ?? {}) as Record<string, string>;
    const response = await hashes.setHash(connectionName, key, entries, dbValue);
    return NextResponse.json(response);
  }

  const unsupported: ApiResponse<boolean> = {
    isSuccess: false,
    message: "Unsupported update for key type",
    reasons: [],
    data: false,
  };
  return NextResponse.json(unsupported);
}
