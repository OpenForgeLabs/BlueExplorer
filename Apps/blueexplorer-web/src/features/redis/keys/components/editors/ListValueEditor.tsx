"use client";

import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
import { ListTableEditor } from "@/features/redis/keys/components/table/ListTableEditor";
import { RedisValueEditorHandle } from "@/features/redis/keys/components/editors/RedisValueEditorTypes";

type ListRow = { id: string; value: string };

type ListValueEditorProps = {
  value: string[];
};

export const ListValueEditor = forwardRef<
  RedisValueEditorHandle,
  ListValueEditorProps
>(({ value }, ref) => {
  const [view, setView] = useState<"table" | "raw">("raw");
  const initialRows = Array.isArray(value) ? value : [];
  const [rows, setRows] = useState<ListRow[]>(
    initialRows.map((item) => ({
      id: `row-${Math.random().toString(36).slice(2, 8)}`,
      value: item,
    })),
  );
  const [rawText, setRawText] = useState(
    JSON.stringify(initialRows, null, 2),
  );
  const [parseError, setParseError] = useState<string | null>(null);
  const lastEditSource = useRef<"raw" | "table" | null>(null);

  useEffect(() => {
    const nextRows = Array.isArray(value) ? value : [];
    setRows(
      nextRows.map((item) => ({
        id: `row-${Math.random().toString(36).slice(2, 8)}`,
        value: item,
      })),
    );
    setRawText(JSON.stringify(nextRows, null, 2));
    setParseError(null);
  }, [value]);

  useImperativeHandle(ref, () => ({
    getValue: () => rows.map((row) => row.value),
  }));

  useEffect(() => {
    if (lastEditSource.current === "raw") {
      return;
    }
    if (view !== "table") {
      return;
    }
    setRawText(JSON.stringify(rows.map((row) => row.value), null, 2));
    setParseError(null);
  }, [rows, view]);

  const handleRawChange = (nextValue: string) => {
    lastEditSource.current = "raw";
    setRawText(nextValue);
    try {
      const parsed = JSON.parse(nextValue);
      if (Array.isArray(parsed)) {
        setRows(
          parsed.map((item) => ({
            id: `row-${Math.random().toString(36).slice(2, 8)}`,
            value: String(item),
          })),
        );
        setParseError(null);
      } else {
        setParseError("Raw content is not a JSON array.");
      }
    } catch {
      setParseError("Invalid JSON.");
    } finally {
      lastEditSource.current = null;
    }
  };

  return (
    <div className="flex flex-1 flex-col">
      <div className="flex items-center justify-between border-b border-border-dark bg-surface-dark/50 px-6 py-3">
        <div className="flex flex-wrap items-center gap-3 text-xs font-bold uppercase tracking-widest text-slate-400">
          <span>Values</span>
          <div className="flex items-center gap-1 rounded-lg border border-border-dark bg-background p-0.5">
            <button
              className={`rounded px-3 py-1 text-[10px] font-bold uppercase transition-colors ${
                view === "raw"
                  ? "bg-primary text-white"
                  : "text-slate-400 hover:text-white"
              }`}
              type="button"
              onClick={() => setView("raw")}
            >
              Raw
            </button>
            <button
              className={`rounded px-3 py-1 text-[10px] font-bold uppercase transition-colors ${
                view === "table"
                  ? "bg-primary text-white"
                  : "text-slate-400 hover:text-white"
              }`}
              type="button"
              onClick={() => setView("table")}
            >
              Table
            </button>
          </div>
        </div>
        <span className="text-[10px] uppercase tracking-widest text-slate-500">
          list
        </span>
      </div>

      <div className="custom-scrollbar flex-1 overflow-auto">
        {view === "table" ? (
          <table className="w-full text-left text-sm">
            <thead className="sticky top-0 border-b border-border-dark bg-background text-[10px] font-bold uppercase tracking-wider text-slate-500">
              <tr>
                <th className="px-6 py-3 w-1/3">Index</th>
                <th className="px-6 py-3">Value</th>
                <th className="px-6 py-3 w-16"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border-dark">
              <ListTableEditor
                rows={rows}
                onChange={(index, value) => {
                  lastEditSource.current = "table";
                  setRows((previous) =>
                    previous.map((row, idx) =>
                      idx === index ? { ...row, value } : row,
                    ),
                  );
                }}
                onRemove={(index) =>
                  setRows((previous) =>
                    previous.filter((_, idx) => idx !== index),
                  )
                }
                onAdd={() =>
                  setRows((previous) => [
                    ...previous,
                    { id: `row-${Date.now()}`, value: "" },
                  ])
                }
              />
            </tbody>
          </table>
        ) : (
          <div className="p-6">
            <textarea
              className="min-h-[280px] w-full rounded-lg border border-border-dark bg-background/40 p-4 font-mono text-xs text-slate-200 focus:ring-2 focus:ring-primary/50"
              value={rawText}
              onChange={(event) => handleRawChange(event.target.value)}
            />
            {parseError && (
              <p className="mt-2 text-[11px] text-rose-300">{parseError}</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
});

ListValueEditor.displayName = "ListValueEditor";
