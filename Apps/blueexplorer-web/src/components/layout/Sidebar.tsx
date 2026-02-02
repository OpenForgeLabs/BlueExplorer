import { Button } from "@/components/Button";
import { ResourceType } from "@/lib/types";

type SidebarProps = {
  onNewConnection?: () => void;
  onSelectResource?: (type: ResourceType) => void;
  selectedResource?: ResourceType;
  className?: string;
  collapsed?: boolean;
  onToggleCollapse?: () => void;
};

const DEFAULT_RESOURCE: ResourceType = "all";

const RESOURCE_ITEMS: Array<{
  type: ResourceType;
  label: string;
  icon: string;
}> = [
  { type: "all", label: "All Resources", icon: "apps" },
  { type: "service-bus", label: "Service Bus", icon: "queue" },
  { type: "redis", label: "Redis Cache", icon: "database" },
];

export function Sidebar({
  onNewConnection,
  onSelectResource,
  selectedResource = DEFAULT_RESOURCE,
  className,
  collapsed = false,
  onToggleCollapse,
}: SidebarProps) {
  return (
    <aside
      className={`relative flex-col border-r border-border-dark bg-background ${
        collapsed ? "w-20" : "w-64"
      } ${
        className ?? "hidden lg:flex"
      }`}
    >
      <div
        className={`flex items-center ${
          collapsed ? "justify-center px-0 py-6" : "gap-3 p-6"
        }`}
      >
        <div
          className={`flex h-8 w-8 items-center justify-center rounded ${
            collapsed ? "bg-transparent text-primary" : "bg-primary text-white"
          }`}
        >
          <span className="material-symbols-outlined">hub</span>
        </div>
        <div className={collapsed ? "hidden" : ""}>
          <h1 className="text-lg font-bold leading-none text-slate-100">
            BlueExplorer
          </h1>
        </div>
        {!collapsed && onToggleCollapse ? (
          <span className="ml-auto hidden w-8 lg:block" aria-hidden />
        ) : null}
      </div>
      {onToggleCollapse ? (
        <button
          className={`absolute top-1/2 hidden -translate-y-1/2 items-center justify-center rounded-full border border-border-dark/60 bg-surface-dark/70 text-slate-300 shadow-lg backdrop-blur hover:border-primary lg:flex ${
            collapsed ? "-right-3 h-10 w-10" : "-right-2 h-8 w-8"
          }`}
          type="button"
          onClick={onToggleCollapse}
          title={collapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          <span className="material-symbols-outlined text-[18px]">
            {collapsed ? "chevron_right" : "chevron_left"}
          </span>
        </button>
      ) : null}
      <nav className="flex-1 space-y-1 overflow-y-auto px-4">
        <div
          className={`px-3 py-4 text-[10px] font-bold uppercase tracking-widest text-slate-500 ${
            collapsed ? "hidden" : ""
          }`}
        >
          Resources
        </div>
        {RESOURCE_ITEMS.map((item) => {
          const isActive = selectedResource === item.type;
          const baseClass = collapsed
            ? "flex w-full items-center justify-center rounded-lg px-0 py-2 text-sm transition-colors"
            : "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm transition-colors";
          const activeClass = "bg-primary/15 text-primary";
          const inactiveClass = "text-slate-400 hover:bg-surface-dark";

          return (
            <button
              key={item.type}
              className={`${baseClass} ${isActive ? activeClass : inactiveClass}`}
              onClick={() => onSelectResource?.(item.type)}
              type="button"
              title={item.label}
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              <span className={collapsed ? "hidden" : ""}>{item.label}</span>
            </button>
          );
        })}
        <div
          className={`px-3 py-4 text-[10px] font-bold uppercase tracking-widest text-slate-500 ${
            collapsed ? "hidden" : ""
          }`}
        >
          Management
        </div>
        <a
          className={`flex items-center rounded-lg py-2 text-sm text-slate-400 transition-colors hover:bg-surface-dark ${
            collapsed ? "justify-center px-0" : "gap-3 px-3"
          }`}
          href="#"
          title="Activity Logs"
        >
          <span className="material-symbols-outlined">history</span>
          <span className={collapsed ? "hidden" : ""}>Activity Logs</span>
        </a>
        <a
          className={`flex items-center rounded-lg py-2 text-sm text-slate-400 transition-colors hover:bg-surface-dark ${
            collapsed ? "justify-center px-0" : "gap-3 px-3"
          }`}
          href="#"
          title="Settings"
        >
          <span className="material-symbols-outlined">settings</span>
          <span className={collapsed ? "hidden" : ""}>Settings</span>
        </a>
      </nav>
      <div className={`border-t border-border-dark ${collapsed ? "p-3" : "p-4"}`}>
        <Button
          className={`w-full gap-2 ${collapsed ? "px-0" : ""}`}
          onClick={onNewConnection}
          title="New Connection"
        >
          <span className="material-symbols-outlined text-[20px]">
            add_circle
          </span>
          <span className={collapsed ? "hidden" : ""}>New Connection</span>
        </Button>
      </div>
    </aside>
  );
}
