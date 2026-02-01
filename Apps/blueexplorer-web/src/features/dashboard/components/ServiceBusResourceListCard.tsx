import { ResourceListItemBase } from "@/features/dashboard/components/ResourceListItemBase";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { ServiceBusResourceSummary } from "@/lib/types";

export function ServiceBusResourceListCard({
  resource,
}: {
  resource: ServiceBusResourceSummary;
}) {
  const href = `/servicebus/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceListItemBase
      resource={resource}
      icon="queue"
      href={href}
      metrics={
        <>
          <MetricStat label="Messages" value={resource.messageRate ?? "-"} />
          <MetricStat label="Active Queues" value={resource.activeQueues ?? "-"} />
        </>
      }
    />
  );
}
