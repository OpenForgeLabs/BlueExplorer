import type { ReactNode } from "react";
import Link from "next/link";
import { Card } from "@/components/Card";
import { ResourceSummaryBase } from "@/lib/types";

const STATUS_STYLES: Record<string, string> = {
  connected: "bg-emerald-500",
  warning: "bg-amber-500",
  offline: "bg-rose-500",
};

const ENVIRONMENT_STYLES: Record<string, string> = {
  production: "bg-emerald-500/10 text-emerald-400",
  staging: "bg-amber-500/10 text-amber-400",
  development: "bg-sky-500/10 text-sky-400",
};

type ResourceListItemBaseProps = {
  resource: ResourceSummaryBase;
  icon: string;
  href: string;
  metrics: ReactNode;
};

export function ResourceListItemBase({
  resource,
  icon,
  href,
  metrics,
}: ResourceListItemBaseProps) {
  const statusClass = STATUS_STYLES[resource.status ?? ""] ?? "bg-slate-500";
  const envClass =
    ENVIRONMENT_STYLES[resource.environment ?? ""] ??
    "bg-slate-500/10 text-slate-300";

  return (
    <Link href={href} className="block">
      <Card className="grid cursor-pointer gap-4 md:grid-cols-[minmax(0,1.4fr)_minmax(0,1.6fr)_minmax(0,0.8fr)] md:items-center">
      <div className="flex items-start gap-4">
        <div className="flex size-10 items-center justify-center rounded bg-primary/10 text-primary">
          <span className="material-symbols-outlined text-[26px]">{icon}</span>
        </div>
        <div className="flex-1">
          <div className="mb-2 flex flex-wrap items-center gap-2">
            <h3 className="text-base font-bold text-slate-100">
              {resource.name}
            </h3>
            <span
              className={`inline-flex items-center rounded px-2 py-0.5 text-[10px] font-bold uppercase ${envClass}`}
            >
              {resource.environment ?? "environment"}
            </span>
          </div>
          <p className="truncate font-mono text-xs text-slate-400">
            {resource.endpoint}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3">
        {metrics}
      </div>

      <div className="flex items-center justify-between border-t border-border-dark pt-3 md:flex-col md:items-end md:gap-3 md:border-t-0 md:pt-0">
        <div className="flex items-center gap-1.5">
          <div className={`size-2 rounded-full ${statusClass}`}></div>
          <span className="text-xs text-slate-400">
            {resource.status ?? "unknown"}
          </span>
        </div>
        <span className="material-symbols-outlined text-slate-500">
          chevron_right
        </span>
      </div>
      </Card>
    </Link>
  );
}
