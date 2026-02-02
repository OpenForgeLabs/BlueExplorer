import { ResourceListItemBase } from "@/features/dashboard/components/ResourceListItemBase";
import { ResourceDeleteAction } from "@/features/dashboard/components/ResourceDeleteAction";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { ServiceBusResourceSummary } from "@/lib/types";

export function ServiceBusResourceListCard({
  resource,
  onDelete,
}: {
  resource: ServiceBusResourceSummary;
  onDelete?: (resource: ServiceBusResourceSummary) => Promise<void>;
}) {
  const href = `/servicebus/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceListItemBase
      resource={resource}
      icon="queue"
      href={href}
      actions={
        onDelete ? (
          <ResourceDeleteAction resource={resource} onDelete={onDelete} />
        ) : null
      }
      metrics={
        <>
          <MetricStat label="Messages" value={resource.messageRate ?? "-"} />
          <MetricStat label="Active Queues" value={resource.activeQueues ?? "-"} />
        </>
      }
    />
  );
}
