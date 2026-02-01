import { BaseClient } from "@/infrastructure/baseClient";
import { ApiResponse, ServiceBusResourceSummary } from "@/lib/types";

export class ServiceBusClient {
  constructor(private readonly client: BaseClient) {}

  async getConnections(): Promise<ApiResponse<ServiceBusResourceSummary[]>> {
    if (this.client.isMocked) {
      return {
        isSuccess: true,
        message: "",
        reasons: [],
        data: [
          {
            id: "sb-prod-orders",
            name: "prod-orders-bus",
            type: "service-bus",
            environment: "production",
            endpoint: "sb://prod-orders.servicebus.windows.net",
            messageRate: "1.2k / sec",
            activeQueues: 24,
            status: "connected",
          },
          {
            id: "sb-stg-billing",
            name: "stg-billing-bus",
            type: "service-bus",
            environment: "staging",
            endpoint: "sb://stg-billing.servicebus.windows.net",
            messageRate: "210 / sec",
            activeQueues: 8,
            status: "warning",
          },
          {
            id: "sb-prod-notify",
            name: "prod-notifications",
            type: "service-bus",
            environment: "production",
            endpoint: "sb://prod-notify.servicebus.windows.net",
            messageRate: "560 / sec",
            activeQueues: 16,
            status: "connected",
          },
        ],
      };
    }

    return this.client.get<ServiceBusResourceSummary[]>("/api/connections");
  }

  async getHealth(connectionName: string): Promise<ApiResponse<boolean>> {
    return this.client.get<boolean>(`/api/connections/${connectionName}/health`);
  }
}
