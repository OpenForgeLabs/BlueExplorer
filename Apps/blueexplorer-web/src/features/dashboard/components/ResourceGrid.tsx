import { RedisResourceCard } from "@/features/dashboard/components/RedisResourceCard";
import { RedisResourceListCard } from "@/features/dashboard/components/RedisResourceListCard";
import { ServiceBusResourceCard } from "@/features/dashboard/components/ServiceBusResourceCard";
import { ServiceBusResourceListCard } from "@/features/dashboard/components/ServiceBusResourceListCard";
import {
  RedisResourceSummary,
  ResourceSummary,
  ServiceBusResourceSummary,
} from "@/lib/types";

type ResourceGridProps = {
  resources: ResourceSummary[];
  isLoading: boolean;
  error?: string;
  view?: "grid" | "list";
  onDeleteResource?: (resource: ResourceSummary) => Promise<void>;
};

export function ResourceGrid({
  resources,
  isLoading,
  error,
  view = "grid",
  onDeleteResource,
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
  const renderCard = (resource: ResourceSummary) => {
    if (resource.type === "service-bus") {
      return view === "list" ? (
        <ServiceBusResourceListCard
          resource={resource as ServiceBusResourceSummary}
          onDelete={onDeleteResource as
            | ((resource: ServiceBusResourceSummary) => Promise<void>)
            | undefined}
        />
      ) : (
        <ServiceBusResourceCard
          resource={resource as ServiceBusResourceSummary}
          onDelete={onDeleteResource as
            | ((resource: ServiceBusResourceSummary) => Promise<void>)
            | undefined}
        />
      );
    }

    const redis = resource as RedisResourceSummary;
    return view === "list" ? (
      <RedisResourceListCard
        resource={redis}
        onDelete={onDeleteResource as
          | ((resource: RedisResourceSummary) => Promise<void>)
          | undefined}
      />
    ) : (
      <RedisResourceCard
        resource={redis}
        onDelete={onDeleteResource as
          | ((resource: RedisResourceSummary) => Promise<void>)
          | undefined}
      />
    );
  };

  return (
    <div className={listClassName}>
      {resources.map((resource) => (
        <div key={resource.id}>{renderCard(resource)}</div>
      ))}
    </div>
  );
}
