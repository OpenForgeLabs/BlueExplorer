import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisHashesClient } from "@/infrastructure/redis/clients/RedisHashesClient";
import { RedisKeysClient } from "@/infrastructure/redis/clients/RedisKeysClient";
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
  const rawType = request.nextUrl.searchParams.get("type") ?? "unknown";
  const normalizedType = rawType.toLowerCase();
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

  switch (normalizedType) {
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
  const keysClient = new RedisKeysClient(client);
  const lists = new RedisListsClient(client);
  const sets = new RedisSetsClient(client);
  const zsets = new RedisZSetsClient(client);
  const streams = new RedisStreamsClient(client);

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

  if (body.type === "list") {
    const values = Array.isArray(body.value) ? body.value.map(String) : null;
    if (!values) {
      const response: ApiResponse<boolean> = {
        isSuccess: false,
        message: "Invalid list payload",
        reasons: ["Expected array of strings"],
        data: false,
      };
      return NextResponse.json(response, { status: 400 });
    }
    await keysClient.deleteKey(connectionName, key, key, dbValue);
    if (values.length === 0) {
      return NextResponse.json({
        isSuccess: true,
        message: "List cleared",
        reasons: [],
        data: true,
      } satisfies ApiResponse<boolean>);
    }
    const response = await lists.pushList(
      connectionName,
      key,
      values,
      false,
      dbValue,
    );
    return NextResponse.json({
      ...response,
      data: response.isSuccess,
    } satisfies ApiResponse<boolean>);
  }

  if (body.type === "set") {
    const members = Array.isArray(body.value)
      ? body.value.map(String)
      : null;
    if (!members) {
      const response: ApiResponse<boolean> = {
        isSuccess: false,
        message: "Invalid set payload",
        reasons: ["Expected array of strings"],
        data: false,
      };
      return NextResponse.json(response, { status: 400 });
    }
    await keysClient.deleteKey(connectionName, key, key, dbValue);
    if (members.length === 0) {
      return NextResponse.json({
        isSuccess: true,
        message: "Set cleared",
        reasons: [],
        data: true,
      } satisfies ApiResponse<boolean>);
    }
    const response = await sets.addSet(connectionName, key, members, dbValue);
    return NextResponse.json({
      ...response,
      data: response.isSuccess,
    } satisfies ApiResponse<boolean>);
  }

  if (body.type === "zset") {
    const entries = Array.isArray(body.value) ? body.value : null;
    if (!entries) {
      const response: ApiResponse<boolean> = {
        isSuccess: false,
        message: "Invalid zset payload",
        reasons: ["Expected array of entries"],
        data: false,
      };
      return NextResponse.json(response, { status: 400 });
    }
    await keysClient.deleteKey(connectionName, key, key, dbValue);
    if (entries.length === 0) {
      return NextResponse.json({
        isSuccess: true,
        message: "ZSet cleared",
        reasons: [],
        data: true,
      } satisfies ApiResponse<boolean>);
    }
    const response = await zsets.addZSet(
      connectionName,
      key,
      entries,
      dbValue,
    );
    return NextResponse.json({
      ...response,
      data: response.isSuccess,
    } satisfies ApiResponse<boolean>);
  }

  if (body.type === "stream") {
    const entries = Array.isArray(body.value) ? body.value : null;
    if (!entries) {
      const response: ApiResponse<boolean> = {
        isSuccess: false,
        message: "Invalid stream payload",
        reasons: ["Expected array of entries"],
        data: false,
      };
      return NextResponse.json(response, { status: 400 });
    }
    for (const entry of entries) {
      await streams.addEntry(
        connectionName,
        key,
        entry.values ?? {},
        entry.id && String(entry.id).trim() ? entry.id : null,
        dbValue,
      );
    }
    const response: ApiResponse<boolean> = {
      isSuccess: true,
      message: "Stream updated",
      reasons: [],
      data: true,
    };
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
