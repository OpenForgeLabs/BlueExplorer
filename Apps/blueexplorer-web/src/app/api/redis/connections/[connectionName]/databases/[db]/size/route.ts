import { NextRequest, NextResponse } from "next/server";
import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import { RedisServerClient } from "@/infrastructure/redis/clients/RedisServerClient";
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

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ connectionName: string; db: string }> },
) {
  const { connectionName, db } = await params;
  const dbIndex = Number(db);
  const { client, useMocks } = getClient(request);

  if (Number.isNaN(dbIndex) || dbIndex < 0) {
    const invalid: ApiResponse<number> = {
      isSuccess: false,
      message: "Invalid database index.",
      reasons: ["Database index must be zero or greater."],
      data: 0,
    };
    return NextResponse.json(invalid, { status: 400 });
  }

  if (useMocks) {
    const mock: ApiResponse<number> = {
      isSuccess: true,
      message: "",
      reasons: [],
      data: Math.floor(Math.random() * 2000),
    };
    return NextResponse.json(mock);
  }

  const response = await new RedisServerClient(client).getDatabaseSize(
    connectionName,
    dbIndex,
  );

  return NextResponse.json(response);
}
