"use client";

import { useMemo, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Sidebar } from "@/components/layout/Sidebar";
import { Topbar } from "@/components/layout/Topbar";
import { NewConnectionModal } from "@/components/modals/NewConnectionModal";
import { Breadcrumbs } from "@/components/nav/Breadcrumbs";
import { ResourceType } from "@/lib/types";

type AppShellProps = {
  children: React.ReactNode;
};

const parseResourceType = (value: string | null): ResourceType => {
  if (value === "redis" || value === "service-bus") {
    return value;
  }
  return "all";
};

export function AppShell({ children }: AppShellProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isNewConnectionOpen, setIsNewConnectionOpen] = useState(false);

  const selectedResource = useMemo<ResourceType>(() => {
    if (pathname?.startsWith("/redis")) {
      return "redis";
    }
    if (pathname?.startsWith("/servicebus")) {
      return "service-bus";
    }
    return parseResourceType(searchParams?.get("resource") ?? null);
  }, [pathname, searchParams]);

  const handleSelectResource = (type: ResourceType) => {
    const params = new URLSearchParams(searchParams?.toString());
    if (type === "all") {
      params.delete("resource");
    } else {
      params.set("resource", type);
    }
    const query = params.toString();
    router.push(`/${query ? `?${query}` : ""}`);
  };

  return (
    <div className="flex min-h-screen bg-background text-slate-100">
      <Sidebar
        selectedResource={selectedResource}
        onSelectResource={handleSelectResource}
        onNewConnection={() => setIsNewConnectionOpen(true)}
      />
      <main className="flex min-h-screen flex-1 flex-col overflow-hidden">
        <Topbar />
        <div className="bg-background/50 px-6 pt-6 lg:px-8">
          <Breadcrumbs />
        </div>
        {children}
      </main>
      <NewConnectionModal
        open={isNewConnectionOpen}
        onClose={() => setIsNewConnectionOpen(false)}
      />
    </div>
  );
}
