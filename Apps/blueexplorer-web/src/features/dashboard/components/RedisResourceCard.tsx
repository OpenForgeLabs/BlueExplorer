import { ResourceCardBase } from "@/features/dashboard/components/ResourceCardBase";
import { MetricStat } from "@/features/dashboard/components/MetricStat";
import { RedisResourceSummary } from "@/lib/types";

export function RedisResourceCard({
  resource,
}: {
  resource: RedisResourceSummary;
}) {
  const href = `/redis/${encodeURIComponent(resource.name)}`;

  return (
    <ResourceCardBase resource={resource} icon="database" href={href}>
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
