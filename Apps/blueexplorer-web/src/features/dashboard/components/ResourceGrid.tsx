import { RedisResourceCard } from "@/features/dashboard/components/RedisResourceCard";
import { RedisResourceListCard } from "@/features/dashboard/components/RedisResourceListCard";
import { ServiceBusResourceCard } from "@/features/dashboard/components/ServiceBusResourceCard";
import { ServiceBusResourceListCard } from "@/features/dashboard/components/ServiceBusResourceListCard";
import { RedisResourceSummary, ResourceSummary, ServiceBusResourceSummary } from "@/lib/types";

const GRID_CARD_MAP: Record<
  ResourceSummary["type"],
  (resource: ResourceSummary) => JSX.Element
> = {
  "service-bus": (resource) => (
    <ServiceBusResourceCard resource={resource as ServiceBusResourceSummary} />
  ),
  redis: (resource) => (
    <RedisResourceCard resource={resource as RedisResourceSummary} />
  ),
};

const LIST_CARD_MAP: Record<
  ResourceSummary["type"],
  (resource: ResourceSummary) => JSX.Element
> = {
  "service-bus": (resource) => (
    <ServiceBusResourceListCard
      resource={resource as ServiceBusResourceSummary}
    />
  ),
  redis: (resource) => (
    <RedisResourceListCard resource={resource as RedisResourceSummary} />
  ),
};

type ResourceGridProps = {
  resources: ResourceSummary[];
  isLoading: boolean;
  error?: string;
  view?: "grid" | "list";
};

export function ResourceGrid({
  resources,
  isLoading,
  error,
  view = "grid",
}: ResourceGridProps) {
  if (isLoading) {
    return (
      <div className="rounded-xl border border-dashed border-border-dark p-8 text-center text-sm text-slate-400">
        Loading resources...
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl border border-dashed border-rose-500/40 bg-rose-500/10 p-8 text-center text-sm text-rose-200">
        {error}
      </div>
    );
  }

  if (!resources.length) {
    return (
      <div className="rounded-xl border border-dashed border-border-dark p-8 text-center text-sm text-slate-400">
        No resources found for this filter.
      </div>
    );
  }

  const listClassName =
    view === "list"
      ? "grid grid-cols-1 gap-4"
      : "grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3";
  const cardMap = view === "list" ? LIST_CARD_MAP : GRID_CARD_MAP;

  return (
    <div className={listClassName}>
      {resources.map((resource) => (
        <div key={resource.id}>{cardMap[resource.type](resource)}</div>
      ))}
    </div>
  );
}
