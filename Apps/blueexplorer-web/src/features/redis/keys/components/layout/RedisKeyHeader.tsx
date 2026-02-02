"use client";

import { RedisKeyType } from "@/lib/types";

type RedisKeyHeaderProps = {
  selectedKey: string | null;
  selectedType: RedisKeyType;
  selectedValue: unknown;
  nameDraft: string;
  isRenaming: boolean;
  isSaving: boolean;
  canSave: boolean;
  ttlValue: string;
  ttlError?: string | null;
  saveError?: string | null;
  typeDescription: string;
  isLocalKey: boolean;
  onRenameToggle: () => void;
  onNameChange: (value: string) => void;
  onTtlChange: (value: string) => void;
  onRenameConfirm: () => void;
  onRefreshValue: () => void;
  onSave: () => void;
  onDelete: () => void;
  onTypeChange: (type: RedisKeyType) => void;
};

export function RedisKeyHeader({
  selectedKey,
  selectedType,
  selectedValue,
  nameDraft,
  isRenaming,
  isSaving,
  canSave,
  ttlValue,
  ttlError,
  saveError,
  typeDescription,
  isLocalKey,
  onRenameToggle,
  onNameChange,
  onTtlChange,
  onRenameConfirm,
  onRefreshValue,
  onSave,
  onDelete,
  onTypeChange,
}: RedisKeyHeaderProps) {
  const formatSize = (value?: unknown) => {
    if (!value) {
      return "-";
    }
    if (typeof value === "string") {
      return `${value.length} B`;
    }
    if (Array.isArray(value)) {
      return `${value.length} items`;
    }
    if (typeof value === "object") {
      return `${Object.keys(value as Record<string, unknown>).length} fields`;
    }
    return "-";
  };

  return (
    <div className="border-b border-border-dark p-6">
      <div className="flex flex-col gap-4">
        <div className="flex min-w-0 flex-nowrap items-center justify-between gap-3">
          <div className="group flex min-w-0 items-center gap-2">
            {isRenaming ? (
              <div className="flex items-center gap-2">
                <input
                  className="min-w-[220px] rounded-md border border-border-dark bg-background px-3 py-2 text-base font-semibold text-slate-100 focus:ring-2 focus:ring-primary/40"
                  value={nameDraft}
                  onChange={(event) => onNameChange(event.target.value)}
                />
                <button
                  className="rounded-md border border-emerald-500/40 bg-emerald-500/10 px-2 py-2 text-emerald-300 hover:bg-emerald-500 hover:text-white"
                  type="button"
                  onClick={onRenameConfirm}
                >
                  <span className="material-symbols-outlined text-[18px]">
                    check
                  </span>
                </button>
              </div>
            ) : (
              <h3 className="min-w-0 truncate text-lg font-bold leading-tight text-slate-100">
                {selectedKey ?? "Select a key"}
              </h3>
            )}
            {isLocalKey && (
              <select
                className="rounded-md border border-border-dark bg-background px-2 py-1 text-[11px] uppercase text-slate-200"
                value={selectedType ?? "string"}
                onChange={(event) =>
                  onTypeChange(event.target.value as RedisKeyType)
                }
              >
                <option value="string">string</option>
                <option value="hash">hash</option>
                <option value="list">list</option>
                <option value="set">set</option>
                <option value="zset">zset</option>
                <option value="stream">stream</option>
              </select>
            )}
            {!isRenaming && (
              <button
                className="rounded p-1 text-slate-500 opacity-0 transition-opacity group-hover:opacity-100"
                type="button"
                onClick={onRenameToggle}
              >
                <span className="material-symbols-outlined text-[18px]">
                  edit
                </span>
              </button>
            )}
          </div>

          <div className="ml-auto flex flex-nowrap items-center gap-2">
            <button
              className="flex h-9 w-9 items-center justify-center rounded border border-border-dark bg-surface-dark text-slate-200 hover:border-primary"
              type="button"
              onClick={onRefreshValue}
              title="Refresh"
            >
              <span className="material-symbols-outlined text-[16px]">
                refresh
              </span>
            </button>
            <button
              className="flex h-9 w-9 items-center justify-center rounded bg-primary text-white disabled:cursor-not-allowed disabled:opacity-60"
              type="button"
              disabled={isSaving || !canSave}
              onClick={onSave}
              title={isSaving ? "Saving" : "Save changes"}
            >
              <span className="material-symbols-outlined text-[16px]">
                {isSaving ? "hourglass_top" : "save"}
              </span>
            </button>
            <button
              className="flex h-9 w-9 items-center justify-center rounded-lg border border-rose-500/40 bg-rose-500/10 text-rose-300 transition-colors hover:bg-rose-500 hover:text-white"
              type="button"
              onClick={onDelete}
              title="Delete"
            >
              <span className="material-symbols-outlined text-[18px]">
                delete
              </span>
            </button>
            <div className="flex h-9 items-center gap-2 rounded-lg border border-border-dark bg-surface-dark/60 px-3 text-[11px] text-slate-300">
              <span className="material-symbols-outlined text-[16px] text-amber-400">
                timer
              </span>
              <span className="uppercase tracking-widest text-slate-400">
                TTL
              </span>
              <input
                className="h-7 w-12 rounded border border-border-dark bg-background px-2 text-center text-xs font-mono text-slate-100"
                type="text"
                value={ttlValue}
                onChange={(event) => onTtlChange(event.target.value)}
                placeholder="-"
              />
            </div>
          </div>
        </div>

        {saveError && (
          <p className="text-right text-[11px] text-rose-300">{saveError}</p>
        )}

        {ttlError && (
          <p className="text-right text-[11px] text-rose-300">{ttlError}</p>
        )}

        <div className="flex flex-wrap items-center gap-4 text-xs text-slate-400">
          <div>
            <span className="block uppercase tracking-widest">Type</span>
            <span
              className="text-sm font-semibold text-emerald-400"
              title={typeDescription}
            >
              {selectedType ?? "unknown"}
            </span>
          </div>
          <div className="h-8 w-px bg-border-dark"></div>
          <div>
            <span className="block uppercase tracking-widest">Memory</span>
            <span className="text-sm font-semibold">
              {selectedValue ? formatSize(selectedValue) : "-"}
            </span>
          </div>
          <div className="h-8 w-px bg-border-dark"></div>
          <div>
            <span className="block uppercase tracking-widest">Encoding</span>
            <span className="text-sm font-semibold font-mono">-</span>
          </div>
        </div>
      </div>
    </div>
  );
}
