"use client";

type SetRow = { id: string; value: string };

type SetTableEditorProps = {
  rows: SetRow[];
  onChange: (index: number, value: string) => void;
  onRemove: (index: number) => void;
  onAdd: () => void;
  hasDuplicates: boolean;
};

export function SetTableEditor({
  rows,
  onChange,
  onRemove,
  onAdd,
  hasDuplicates,
}: SetTableEditorProps) {
  return (
    <tr>
      <td className="px-6 py-4" colSpan={3}>
        <div className="rounded-lg border border-border-dark bg-background/40 p-4">
          <div className="mb-3 flex items-center justify-between text-xs text-slate-400">
            <span className="uppercase tracking-widest">Unique Members</span>
            <span className="font-mono">{rows.length} members</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {rows.length === 0 && (
              <p className="text-xs text-slate-500">
                No set members available.
              </p>
            )}
            {rows.map((row, index) => {
              const isDuplicate =
                row.value &&
                rows.filter((item) => item.value === row.value).length > 1;
              return (
                <div
                  key={row.id}
                  className={`group flex items-center gap-2 rounded-full border px-3 py-1 text-xs font-mono ${
                    isDuplicate
                      ? "border-rose-500/50 bg-rose-500/10 text-rose-300"
                      : "border-border-dark bg-surface-dark/40 text-slate-200"
                  }`}
                >
                  <input
                    className="w-32 bg-transparent text-xs text-inherit focus:outline-none"
                    value={row.value}
                    onChange={(event) => onChange(index, event.target.value)}
                  />
                  <button
                    className="opacity-0 transition-opacity group-hover:opacity-100"
                    type="button"
                    onClick={() => onRemove(index)}
                  >
                    <span className="material-symbols-outlined text-[16px] text-slate-500 hover:text-rose-400">
                      delete
                    </span>
                  </button>
                </div>
              );
            })}
            <button
              className="flex items-center gap-1 text-xs font-bold text-primary hover:underline"
              type="button"
              onClick={onAdd}
            >
              <span className="material-symbols-outlined text-[16px]">
                add_circle
              </span>
              Add member
            </button>
          </div>
          {hasDuplicates && (
            <p className="mt-2 text-[11px] text-rose-300">
              Set members must be unique.
            </p>
          )}
        </div>
      </td>
    </tr>
  );
}
