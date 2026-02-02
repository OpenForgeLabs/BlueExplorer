import { ResourceListItemBase } from "@/features/dashboard/components/ResourceListItemBase";
import { ResourceDeleteAction } from "@/features/dashboard/components/ResourceDeleteAction";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { RedisResourceSummary } from "@/lib/types";

export function RedisResourceListCard({
  resource,
  onDelete,
}: {
  resource: RedisResourceSummary;
  onDelete?: (resource: RedisResourceSummary) => Promise<void>;
}) {
  const href = `/redis/${encodeURIComponent(resource.name)}/keys`;

  return (
    <ResourceListItemBase
      resource={resource}
      icon="database"
      href={href}
      actions={
        onDelete ? (
          <ResourceDeleteAction resource={resource} onDelete={onDelete} />
        ) : null
      }
      metrics={
        <>
          <MetricStat label="Ops / Sec" value={resource.opsRate ?? "-"} />
          <MetricStat label="Memory Usage" value={resource.memoryUsage ?? "-"} />
          <MetricStat
            className="col-span-2"
            label="Total Keys"
            value={resource.keyCount ?? "-"}
          />
        </>
      }
    />
  );
}
