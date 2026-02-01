import { ApiResponse, ResourceCatalog, ResourceSummary } from "@/lib/types";

export async function fetchAllResources(): Promise<ApiResponse<ResourceSummary[]>> {
  const response = await fetch("/api/resources?type=all", {
    cache: "no-store",
  });

  if (!response.ok) {
    return {
      isSuccess: false,
      message: "Failed to load resources",
      reasons: [response.statusText],
      data: [],
    };
  }

  const payload = (await response.json()) as ApiResponse<ResourceCatalog>;

  if (!payload.isSuccess) {
    return {
      isSuccess: false,
      message: payload.message,
      reasons: payload.reasons,
      data: [],
    };
  }

  return {
    isSuccess: true,
    message: payload.message,
    reasons: payload.reasons,
    data: [...payload.data.serviceBus, ...payload.data.redis],
  };
}
