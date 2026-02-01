import { ResourceCardBase } from "@/features/dashboard/components/ResourceCardBase";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { ServiceBusResourceSummary } from "@/lib/types";

export function ServiceBusResourceCard({
  resource,
}: {
  resource: ServiceBusResourceSummary;
}) {
  const href = `/servicebus/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceCardBase resource={resource} icon="queue" href={href}>
      <div className="mb-6 grid grid-cols-2 gap-4">
        <MetricStat label="Messages" value={resource.messageRate ?? "-"} />
        <MetricStat label="Active Queues" value={resource.activeQueues ?? "-"} />
      </div>
    </ResourceCardBase>
  );
}
