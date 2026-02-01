import { ApiResponse } from "@/lib/types";

export type BaseClientConfig = {
  baseUrl: string;
  useMocks?: boolean;
};

export class BaseClient {
  private readonly baseUrl: string;
  private readonly useMocks: boolean;

  constructor({ baseUrl, useMocks }: BaseClientConfig) {
    this.baseUrl = baseUrl.replace(/\/$/, "");
    this.useMocks = Boolean(useMocks);
  }

  get isMocked() {
    return this.useMocks;
  }

  async request<T>(
    path: string,
    init: RequestInit,
  ): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${this.baseUrl}${path}`, {
        ...init,
        cache: "no-store",
      });
      if (!response.ok) {
        return {
          isSuccess: false,
          message: "Request failed",
          reasons: [response.statusText],
          data: undefined as T,
        };
      }

      return (await response.json()) as ApiResponse<T>;
    } catch (error) {
      return {
        isSuccess: false,
        message: "Request failed",
        reasons: [error instanceof Error ? error.message : "Unknown error"],
        data: undefined as T,
      };
    }
  }

  async get<T>(path: string): Promise<ApiResponse<T>> {
    return this.request<T>(path, { method: "GET" });
  }

  async post<T, TBody>(path: string, body: TBody): Promise<ApiResponse<T>> {
    return this.request<T>(path, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  }

  async put<T, TBody>(path: string, body: TBody): Promise<ApiResponse<T>> {
    return this.request<T>(path, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  }

  async delete<T>(path: string): Promise<ApiResponse<T>> {
    return this.request<T>(path, { method: "DELETE" });
  }
}
