import { ResourceCardBase } from "@/features/dashboard/components/ResourceCardBase";
import { ResourceDeleteAction } from "@/features/dashboard/components/ResourceDeleteAction";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { RedisResourceSummary } from "@/lib/types";

export function RedisResourceCard({
  resource,
  onDelete,
}: {
  resource: RedisResourceSummary;
  onDelete?: (resource: RedisResourceSummary) => Promise<void>;
}) {
  const href = `/redis/${encodeURIComponent(resource.name)}/keys`;

  return (
    <ResourceCardBase
      resource={resource}
      icon="database"
      href={href}
      actions={
        onDelete ? (
          <ResourceDeleteAction resource={resource} onDelete={onDelete} />
        ) : null
      }
    >
      <div className="mb-6 grid grid-cols-2 gap-4">
        <MetricStat label="Ops / Sec" value={resource.opsRate ?? "-"} />
        <MetricStat label="Memory Usage" value={resource.memoryUsage ?? "-"} />
        <MetricStat
          className="col-span-2"
          label="Total Keys"
          value={resource.keyCount ?? "-"}
        />
      </div>
    </ResourceCardBase>
  );
}
