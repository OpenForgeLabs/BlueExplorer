import { RedisBaseClient } from "@/infrastructure/redis/RedisBaseClient";
import {
  ApiResponse,
  RedisConnectionInfo,
  RedisConnectionUpsertRequest,
} from "@/lib/types";

export class RedisConnectionsClient {
  constructor(private readonly client: RedisBaseClient) {}

  getConnections(): Promise<ApiResponse<RedisConnectionInfo[]>> {
    return this.client.get<RedisConnectionInfo[]>("/connections");
  }

  getConnection(
    connectionName: string,
  ): Promise<ApiResponse<RedisConnectionUpsertRequest>> {
    return this.client.get<RedisConnectionUpsertRequest>(
      `/connections/${connectionName}`,
    );
  }

  createConnection(
    request: RedisConnectionUpsertRequest,
  ): Promise<ApiResponse<void>> {
    return this.client.post<void, RedisConnectionUpsertRequest>(
      "/connections",
      request,
    );
  }

  updateConnection(
    connectionName: string,
    request: RedisConnectionUpsertRequest,
  ): Promise<ApiResponse<void>> {
    return this.client.put<void, RedisConnectionUpsertRequest>(
      `/connections/${connectionName}`,
      request,
    );
  }

  deleteConnection(connectionName: string): Promise<ApiResponse<void>> {
    return this.client.delete<void>(`/connections/${connectionName}`);
  }

  getHealth(connectionName: string): Promise<ApiResponse<boolean>> {
    return this.client.get<boolean>(`/connections/${connectionName}/health`);
  }

  testConnection(request: RedisConnectionUpsertRequest): Promise<ApiResponse<boolean>> {
    return this.client.post<boolean, RedisConnectionUpsertRequest>(
      "/connections/test",
      request,
    );
  }
}
