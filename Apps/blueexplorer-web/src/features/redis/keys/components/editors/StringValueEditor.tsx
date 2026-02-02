"use client";

import { forwardRef, useEffect, useImperativeHandle, useMemo, useState } from "react";
import { RedisValueEditorHandle } from "@/features/redis/keys/components/editors/RedisValueEditorTypes";
import { StringPreview } from "@/features/redis/keys/components/shared/StringPreview";

type StringValueEditorProps = {
  value: unknown;
};

export const StringValueEditor = forwardRef<
  RedisValueEditorHandle,
  StringValueEditorProps
>(({ value }, ref) => {
  const [view, setView] = useState<"table" | "raw">("raw");
  const [contentFormat, setContentFormat] = useState<"auto" | "json" | "text">(
    "auto",
  );
  const [prettify, setPrettify] = useState(true);
  const initialText = useMemo(() => {
    if (value === null || value === undefined) {
      return "";
    }
    if (typeof value === "string") {
      return value;
    }
    try {
      return JSON.stringify(value, null, 2);
    } catch {
      return String(value);
    }
  }, [value]);
  const [rawText, setRawText] = useState(initialText);

  useEffect(() => {
    setRawText(initialText);
  }, [initialText]);

  useImperativeHandle(ref, () => ({
    getValue: () => rawText,
  }));

  const handlePrettify = () => {
    if (contentFormat === "text") {
      return;
    }
    try {
      const parsed = JSON.parse(rawText);
      setRawText(JSON.stringify(parsed, null, prettify ? 2 : undefined));
    } catch {
      // ignore invalid JSON
    }
  };

  const handleFormatChange = (format: "auto" | "json" | "text") => {
    setContentFormat(format);
    if (format === "text") {
      return;
    }
    try {
      const parsed = JSON.parse(rawText);
      setRawText(JSON.stringify(parsed, null, prettify ? 2 : undefined));
    } catch {
      // ignore invalid JSON
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
          string
        </span>
      </div>

      <div className="custom-scrollbar flex-1 overflow-auto">
        {view === "table" ? (
          <table className="w-full text-left text-sm">
            <thead className="sticky top-0 border-b border-border-dark bg-background text-[10px] font-bold uppercase tracking-wider text-slate-500">
              <tr>
                <th className="px-6 py-3 w-1/3">Key</th>
                <th className="px-6 py-3">Value</th>
                <th className="px-6 py-3 w-16"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border-dark">
              <StringPreview preview={rawText || "—"} />
            </tbody>
          </table>
        ) : (
          <div className="p-6">
            <div className="mb-4 flex flex-wrap items-center gap-3">
              <div className="flex items-center gap-2 rounded border border-border-dark bg-background px-2 py-1 text-[11px] text-slate-400">
                <span className="material-symbols-outlined text-[14px]">
                  tune
                </span>
                <select
                  className="bg-transparent text-[11px] text-slate-300 focus:ring-0"
                  value={contentFormat}
                  onChange={(event) =>
                    handleFormatChange(
                      event.target.value as "auto" | "json" | "text",
                    )
                  }
                >
                  <option value="auto">Auto</option>
                  <option value="json">JSON</option>
                  <option value="text">Text</option>
                </select>
              </div>
              <button
                className={`rounded border border-border-dark px-3 py-1 text-[11px] font-bold uppercase ${
                  prettify ? "bg-primary/20 text-primary" : "text-slate-400"
                }`}
                type="button"
                onClick={() => {
                  setPrettify((prev) => !prev);
                  handlePrettify();
                }}
              >
                Prettify
              </button>
            </div>
            <textarea
              className="min-h-[280px] w-full rounded-lg border border-border-dark bg-background/40 p-4 font-mono text-xs text-slate-200 focus:ring-2 focus:ring-primary/50"
              value={rawText}
              onChange={(event) => setRawText(event.target.value)}
            />
          </div>
        )}
      </div>
    </div>
  );
});

StringValueEditor.displayName = "StringValueEditor";
