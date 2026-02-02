"use client";

type ListRow = { id: string; value: string };

type ListTableEditorProps = {
  rows: ListRow[];
  onChange: (index: number, value: string) => void;
  onRemove: (index: number) => void;
  onAdd: () => void;
};

export function ListTableEditor({
  rows,
  onChange,
  onRemove,
  onAdd,
}: ListTableEditorProps) {
  return (
    <tr>
      <td className="px-6 py-4" colSpan={3}>
        <div className="rounded-lg border border-border-dark bg-background/40 p-4">
          <div className="mb-3 flex items-center justify-between text-xs text-slate-400">
            <span className="uppercase tracking-widest">Ordered Items</span>
            <span className="font-mono">{rows.length} items</span>
          </div>
          <div className="flex flex-col gap-2">
            {rows.length === 0 && (
              <p className="text-xs text-slate-500">No list items available.</p>
            )}
            {rows.map((row, index) => (
              <div
                key={row.id}
                className="group flex items-center gap-3 rounded-md border border-border-dark/70 bg-surface-dark/40 px-3 py-2"
              >
                <span className="text-xs text-slate-500">{index}</span>
                <input
                  className="flex-1 bg-transparent font-mono text-xs text-slate-200 focus:outline-none"
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
            ))}
            <button
              className="mt-2 flex items-center gap-1 text-xs font-bold text-primary hover:underline"
              type="button"
              onClick={onAdd}
            >
              <span className="material-symbols-outlined text-[16px]">
                add_circle
              </span>
              Add new item
            </button>
          </div>
        </div>
      </td>
    </tr>
  );
}
