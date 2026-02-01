"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { Fragment, useMemo } from "react";

type Crumb = {
  label: string;
  href?: string;
};

const formatLabel = (value: string) =>
  value.replace(/-/g, " ").replace(/\b\w/g, (char) => char.toUpperCase());

export function Breadcrumbs() {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const crumbs = useMemo<Crumb[]>(() => {
    const items: Crumb[] = [{ label: "Resources", href: "/" }];

    if (!pathname || pathname === "/") {
      const resource = searchParams?.get("resource");
      if (resource) {
        items.push({ label: formatLabel(resource) });
      }
      return items;
    }

    if (pathname.startsWith("/redis")) {
      items.push({ label: "Redis", href: "/?resource=redis" });
      const parts = pathname.split("/").filter(Boolean);
      if (parts[1]) {
        const connectionName = decodeURIComponent(parts[1]);
        items.push({
          label: connectionName,
          href: `/redis/${encodeURIComponent(connectionName)}`,
        });
      }
      if (parts[2]) {
        items.push({ label: formatLabel(parts[2]) });
      }
      return items;
    }

    if (pathname.startsWith("/servicebus")) {
      items.push({ label: "Service Bus", href: "/?resource=service-bus" });
      const parts = pathname.split("/").filter(Boolean);
      if (parts[1]) {
        const connectionName = decodeURIComponent(parts[1]);
        items.push({
          label: connectionName,
          href: `/servicebus/${encodeURIComponent(connectionName)}`,
        });
      }
      return items;
    }

    const fallbackParts = pathname.split("/").filter(Boolean);
    fallbackParts.forEach((part) => items.push({ label: formatLabel(part) }));
    return items;
  }, [pathname, searchParams]);

  return (
    <nav className="flex items-center gap-2 text-sm text-slate-400">
      {crumbs.map((crumb, index) => (
        <Fragment key={`${crumb.label}-${index}`}>
          {crumb.href ? (
            <Link className="transition-colors hover:text-primary" href={crumb.href}>
              {crumb.label}
            </Link>
          ) : (
            <span className="font-medium text-slate-100">{crumb.label}</span>
          )}
          {index < crumbs.length - 1 && (
            <span className="material-symbols-outlined text-[16px]">
              chevron_right
            </span>
          )}
        </Fragment>
      ))}
    </nav>
  );
}
