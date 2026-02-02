"use client";

type ZsetRow = { id: string; member: string; score: string };

type ZsetTableEditorProps = {
  rows: ZsetRow[];
  onChange: (index: number, key: "member" | "score", value: string) => void;
  onRemove: (index: number) => void;
  onAdd: () => void;
  empty: boolean;
};

export function ZsetTableEditor({
  rows,
  onChange,
  onRemove,
  onAdd,
  empty,
}: ZsetTableEditorProps) {
  return (
    <>
      {empty && (
        <tr>
          <td className="px-6 py-4" colSpan={3}>
            <p className="text-xs text-slate-500">
              No sorted set members available.
            </p>
          </td>
        </tr>
      )}
      {rows.map((entry, index) => (
        <tr
          key={entry.id}
          className="group transition-colors hover:bg-surface-dark/60"
        >
          <td className="px-6 py-3 font-mono text-xs text-primary">
            <input
              className="w-full bg-transparent font-mono text-xs text-primary focus:outline-none"
              value={entry.member}
              onChange={(event) =>
                onChange(index, "member", event.target.value)
              }
            />
          </td>
          <td className="px-6 py-3 text-xs text-slate-200">
            <input
              className="w-full bg-transparent text-xs text-slate-200 focus:outline-none"
              value={entry.score}
              onChange={(event) =>
                onChange(index, "score", event.target.value)
              }
            />
          </td>
          <td className="px-6 py-3 text-right">
            <button
              className="opacity-0 transition-opacity group-hover:opacity-100"
              type="button"
              onClick={() => onRemove(index)}
            >
              <span className="material-symbols-outlined text-[16px] text-slate-500 hover:text-rose-400">
                delete
              </span>
            </button>
          </td>
        </tr>
      ))}
      <tr>
        <td className="px-6 py-4" colSpan={3}>
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
        </td>
      </tr>
    </>
  );
}
