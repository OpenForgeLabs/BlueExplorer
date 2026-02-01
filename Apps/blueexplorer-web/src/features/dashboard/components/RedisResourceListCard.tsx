import { ResourceListItemBase } from "@/features/dashboard/components/ResourceListItemBase";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { RedisResourceSummary } from "@/lib/types";

export function RedisResourceListCard({
  resource,
}: {
  resource: RedisResourceSummary;
}) {
  const href = `/redis/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceListItemBase
      resource={resource}
      icon="database"
      href={href}
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
