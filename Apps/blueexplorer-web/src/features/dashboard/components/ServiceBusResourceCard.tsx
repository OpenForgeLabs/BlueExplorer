import { ResourceCardBase } from "@/features/dashboard/components/ResourceCardBase";
import { ResourceDeleteAction } from "@/features/dashboard/components/ResourceDeleteAction";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { ServiceBusResourceSummary } from "@/lib/types";

export function ServiceBusResourceCard({
  resource,
  onDelete,
}: {
  resource: ServiceBusResourceSummary;
  onDelete?: (resource: ServiceBusResourceSummary) => Promise<void>;
}) {
  const href = `/servicebus/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceCardBase
      resource={resource}
      icon="queue"
      href={href}
      actions={
        onDelete ? (
          <ResourceDeleteAction resource={resource} onDelete={onDelete} />
        ) : null
      }
    >
      <div className="mb-6 grid grid-cols-2 gap-4">
        <MetricStat label="Messages" value={resource.messageRate ?? "-"} />
        <MetricStat label="Active Queues" value={resource.activeQueues ?? "-"} />
      </div>
    </ResourceCardBase>
  );
}
