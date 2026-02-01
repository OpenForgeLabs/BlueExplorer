import { ApiResponse } from "@/lib/types";

export type ResourceHealthRequestItem = {
  type: "service-bus" | "redis";
  name: string;
};

export type ResourceHealthResponse = {
  statuses: Record<string, boolean>;
};

export async function fetchResourceHealth(
  resources: ResourceHealthRequestItem[],
): Promise<ApiResponse<ResourceHealthResponse>> {
  const response = await fetch("/api/resources/health", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ resources }),
  });

  if (!response.ok) {
    return {
      isSuccess: false,
      message: "Failed to refresh health",
      reasons: [response.statusText],
      data: { statuses: {} },
    };
  }

  return (await response.json()) as ApiResponse<ResourceHealthResponse>;
}
