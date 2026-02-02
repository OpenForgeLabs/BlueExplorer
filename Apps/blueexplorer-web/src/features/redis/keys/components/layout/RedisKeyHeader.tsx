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
      <div className="flex flex-col items-start gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="group min-w-0 flex flex-col gap-2">
          <div className="flex flex-wrap items-center gap-2">
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
              <h3 className="text-xl font-bold text-slate-100">
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
          <div className="mt-3 flex flex-wrap items-center gap-4 text-xs text-slate-400">
            <div>
              <span className="block uppercase tracking-widest">Type</span>
              <span className="text-sm font-semibold text-emerald-400">
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
          <p className="w-full text-xs text-slate-500">{typeDescription}</p>
        </div>
        <div className="flex w-full flex-col items-end gap-3 lg:w-auto">
          <div className="flex gap-2">
            <button
              className="flex items-center gap-1 rounded border border-border-dark bg-surface-dark px-3 py-2 text-xs font-bold uppercase text-slate-200 hover:border-primary"
              type="button"
              onClick={onRefreshValue}
            >
              <span className="material-symbols-outlined text-[16px]">
                refresh
              </span>
              Refresh
            </button>
            <button
              className="flex items-center gap-1 rounded bg-primary px-3 py-2 text-xs font-bold uppercase text-white disabled:cursor-not-allowed disabled:opacity-60"
              type="button"
              disabled={isSaving || !canSave}
              onClick={onSave}
            >
              <span className="material-symbols-outlined text-[16px]">
                {isSaving ? "hourglass_top" : "save"}
              </span>
              {isSaving ? "Saving" : "Save Changes"}
            </button>
            <button
              className="flex items-center gap-2 rounded-lg border border-rose-500/40 bg-rose-500/10 px-4 py-2 text-sm font-semibold text-rose-300 transition-colors hover:bg-rose-500 hover:text-white"
              type="button"
              onClick={onDelete}
            >
              <span className="material-symbols-outlined text-[18px]">
                delete
              </span>
              Delete
            </button>
          </div>
          {saveError && (
            <p className="text-right text-[11px] text-rose-300">
              {saveError}
            </p>
          )}
          <div className="w-full rounded-xl border border-border-dark bg-surface-dark/60 px-3 py-2">
            <div className="flex items-center justify-between text-xs text-slate-400">
              <span className="flex items-center gap-2 text-xs font-semibold text-slate-200">
                <span className="material-symbols-outlined text-[16px] text-amber-400">
                  timer
                </span>
                TTL
              </span>
              <input
                className="h-7 w-14 rounded border border-border-dark bg-background px-2 text-center text-xs font-mono text-slate-100"
                type="text"
                value={ttlValue}
                onChange={(event) => onTtlChange(event.target.value)}
                placeholder="-"
              />
            </div>
            {ttlError && (
              <p className="mt-2 text-[11px] text-rose-300">{ttlError}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
