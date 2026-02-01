import { Button } from "@/components/Button";
import { ResourceType } from "@/lib/types";

type SidebarProps = {
  onNewConnection?: () => void;
  onSelectResource?: (type: ResourceType) => void;
  selectedResource?: ResourceType;
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
}: SidebarProps) {
  return (
    <aside className="hidden w-64 flex-col border-r border-border-dark bg-background lg:flex">
      <div className="flex items-center gap-3 p-6">
        <div className="flex h-8 w-8 items-center justify-center rounded bg-primary text-white">
          <span className="material-symbols-outlined">hub</span>
        </div>
        <div>
          <h1 className="text-lg font-bold leading-none text-slate-100">
            BlueExplorer
          </h1>
        </div>
      </div>
      <nav className="flex-1 space-y-1 overflow-y-auto px-4">
        <div className="px-3 py-4 text-[10px] font-bold uppercase tracking-widest text-slate-500">
          Resources
        </div>
        {RESOURCE_ITEMS.map((item) => {
          const isActive = selectedResource === item.type;
          const baseClass =
            "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm transition-colors";
          const activeClass = "bg-primary/15 text-primary";
          const inactiveClass = "text-slate-400 hover:bg-surface-dark";

          return (
            <button
              key={item.type}
              className={`${baseClass} ${isActive ? activeClass : inactiveClass}`}
              onClick={() => onSelectResource?.(item.type)}
              type="button"
            >
              <span className="material-symbols-outlined">{item.icon}</span>
              {item.label}
            </button>
          );
        })}
        <div className="px-3 py-4 text-[10px] font-bold uppercase tracking-widest text-slate-500">
          Management
        </div>
        <a
          className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-slate-400 transition-colors hover:bg-surface-dark"
          href="#"
        >
          <span className="material-symbols-outlined">history</span>
          Activity Logs
        </a>
        <a
          className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-slate-400 transition-colors hover:bg-surface-dark"
          href="#"
        >
          <span className="material-symbols-outlined">settings</span>
          Settings
        </a>
      </nav>
      <div className="border-t border-border-dark p-4">
        <Button className="w-full gap-2" onClick={onNewConnection}>
          <span className="material-symbols-outlined text-[20px]">
            add_circle
          </span>
          New Connection
        </Button>
      </div>
    </aside>
  );
}
