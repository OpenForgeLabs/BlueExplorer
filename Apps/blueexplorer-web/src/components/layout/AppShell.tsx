"use client";

import { useEffect, useMemo, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Sidebar } from "@/components/layout/Sidebar";
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
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);

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
    setIsSidebarOpen(false);
  };

  useEffect(() => {
    setIsSidebarOpen(false);
  }, [pathname]);

  return (
    <div className="flex min-h-screen bg-background text-slate-100">
      {isSidebarOpen ? (
        <button
          className="fixed inset-0 z-40 bg-black/60 lg:hidden"
          type="button"
          onClick={() => setIsSidebarOpen(false)}
          aria-label="Close navigation"
        />
      ) : null}
      <div
        className={`fixed inset-y-0 left-0 z-50 transform transition-transform lg:hidden ${
          isSidebarOpen ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <Sidebar
          selectedResource={selectedResource}
          onSelectResource={handleSelectResource}
          onNewConnection={() => setIsNewConnectionOpen(true)}
          collapsed={false}
          className="flex w-72 shadow-2xl"
        />
      </div>
      <Sidebar
        selectedResource={selectedResource}
        onSelectResource={handleSelectResource}
        onNewConnection={() => setIsNewConnectionOpen(true)}
        collapsed={isSidebarCollapsed}
        onToggleCollapse={() => setIsSidebarCollapsed((prev) => !prev)}
      />
      <main className="flex min-h-screen min-w-0 flex-1 flex-col overflow-x-hidden">
        <div className="flex items-center gap-3 bg-background/50 px-4 pt-6 sm:px-6 lg:px-8">
          <Breadcrumbs />
        </div>
        <div className="flex min-h-0 flex-1 flex-col">{children}</div>
      </main>
      <NewConnectionModal
        open={isNewConnectionOpen}
        onClose={() => setIsNewConnectionOpen(false)}
      />
    </div>
  );
}
