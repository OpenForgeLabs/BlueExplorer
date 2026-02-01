"use client";

import { useEffect, useMemo, useState } from "react";
import { AsyncGate } from "@/components/feedback/AsyncGate";
import { InlineSpinner } from "@/components/feedback/InlineSpinner";
import { useRedisKeys } from "@/features/redis/keys/hooks/useRedisKeys";
import { updateRedisKeyValue } from "@/features/redis/keys/services/redisKeyValueService";
import { RedisKeyType } from "@/lib/types";

const DB_OPTIONS = Array.from({ length: 16 }, (_, idx) => idx);
const DB_COUNTS = [1200, 45, 0, 890, 23, 2, 0, 0, 0, 4, 12, 1, 0, 0, 0, 0];
const TYPE_FILTERS: Array<"all" | RedisKeyType> = [
  "all",
  "string",
  "hash",
  "list",
  "set",
  "zset",
  "stream",
];

const TYPE_BADGE_STYLES: Record<string, string> = {
  string: "bg-blue-500/10 text-blue-300 border-blue-500/30",
  hash: "bg-emerald-500/10 text-emerald-300 border-emerald-500/30",
  list: "bg-amber-500/10 text-amber-300 border-amber-500/30",
  set: "bg-purple-500/10 text-purple-300 border-purple-500/30",
  zset: "bg-pink-500/10 text-pink-300 border-pink-500/30",
  stream: "bg-sky-500/10 text-sky-300 border-sky-500/30",
  unknown: "bg-slate-500/10 text-slate-300 border-slate-500/30",
};

type RedisKeysScreenProps = {
  connectionName: string;
};

export function RedisKeysScreen({ connectionName }: RedisKeysScreenProps) {
  const [pattern, setPattern] = useState("");
  const [db, setDb] = useState<number | "">("");
  const [filterType, setFilterType] = useState<"all" | RedisKeyType>("all");
  const [selectedKey, setSelectedKey] = useState<string | null>(null);
  const [valueView, setValueView] = useState<"table" | "raw">("table");
  const [contentFormat, setContentFormat] = useState<"auto" | "json" | "text">(
    "auto",
  );
  const [prettify, setPrettify] = useState(true);
  const [rawDraft, setRawDraft] = useState("");
  const [hashDraft, setHashDraft] = useState<
    Array<{ field: string; value: string }>
  >([]);
  const [isSaving, setIsSaving] = useState(false);

  const {
    data,
    error,
    isLoading,
    loadKeys,
    keyInfoMap,
    valueMap,
    loadKeyValue,
    nextPage,
    previousPage,
    hasNextPage,
    hasPreviousPage,
  } = useRedisKeys(connectionName);

  const handleSearch = () => {
    loadKeys(
      {
        pattern: pattern || undefined,
        db: db === "" ? undefined : db,
        cursor: 0,
      },
      true,
    );
  };

  useEffect(() => {
    if (db === "") {
      return;
    }
    loadKeys(
      {
        pattern: pattern || undefined,
        db,
        cursor: 0,
      },
      true,
    );
  }, [db]);

  const filteredKeys = useMemo(() => {
    if (filterType === "all") {
      return data.keys;
    }
    return data.keys.filter((key) => keyInfoMap[key]?.type === filterType);
  }, [data.keys, filterType, keyInfoMap]);

  useEffect(() => {
    if (!filteredKeys.length) {
      setSelectedKey(null);
      return;
    }
    setSelectedKey((previous) => previous ?? filteredKeys[0]);
  }, [filteredKeys]);

  useEffect(() => {
    if (!selectedKey) {
      return;
    }
    const info = keyInfoMap[selectedKey];
    if (!info) {
      return;
    }
    loadKeyValue(selectedKey, info.type, db === "" ? undefined : db);
  }, [selectedKey, keyInfoMap, db]);

  const selectedInfo = selectedKey ? keyInfoMap[selectedKey] : undefined;
  const selectedValue = selectedKey ? valueMap[selectedKey] : undefined;

  const resultsLabel = useMemo(() => {
    if (isLoading) return "Loading keys...";
    return `${filteredKeys.length} keys`;
  }, [filteredKeys.length, isLoading]);

  const formatTtl = (ttlSeconds?: number | null) => {
    if (ttlSeconds === null || ttlSeconds === undefined) {
      return "Persist";
    }
    return `${ttlSeconds}s`;
  };

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

  const formatRawValue = () => {
    if (!selectedValue) {
      return "Select a key to view its value.";
    }

    if (contentFormat === "text") {
      if (typeof selectedValue.value === "string") {
        return selectedValue.value;
      }
      return String(selectedValue.value);
    }

    if (contentFormat === "json") {
      return JSON.stringify(
        selectedValue.value,
        null,
        prettify ? 2 : undefined,
      );
    }

    if (typeof selectedValue.value === "string") {
      try {
        const parsed = JSON.parse(selectedValue.value);
        return JSON.stringify(parsed, null, prettify ? 2 : undefined);
      } catch {
        return selectedValue.value;
      }
    }

    return JSON.stringify(
      selectedValue.value,
      null,
      prettify ? 2 : undefined,
    );
  };

  useEffect(() => {
    if (!selectedValue) {
      setRawDraft("");
      setHashDraft([]);
      return;
    }

    if (selectedValue.type === "hash") {
      const entries = Object.entries(selectedValue.value);
      setHashDraft(entries.map(([field, value]) => ({ field, value })));
    } else {
      const raw =
        typeof selectedValue.value === "string"
          ? selectedValue.value
          : JSON.stringify(selectedValue.value, null, 2);
      setRawDraft(raw ?? "");
    }
  }, [selectedKey, selectedValue?.type]);

  const handleAddHashRow = () => {
    setHashDraft((previous) => [...previous, { field: "", value: "" }]);
  };

  const handleHashChange = (
    index: number,
    key: "field" | "value",
    value: string,
  ) => {
    setHashDraft((previous) =>
      previous.map((row, idx) =>
        idx === index ? { ...row, [key]: value } : row,
      ),
    );
  };

  const handleHashRemove = (index: number) => {
    setHashDraft((previous) => previous.filter((_, idx) => idx !== index));
  };

  const handleSave = async () => {
    if (!selectedKey || !selectedInfo) {
      return;
    }
    if (selectedInfo.type !== "string" && selectedInfo.type !== "hash") {
      return;
    }

    setIsSaving(true);
    try {
      if (selectedInfo.type === "string") {
        await updateRedisKeyValue(
          connectionName,
          selectedKey,
          "string",
          rawDraft,
          db === "" ? undefined : db,
        );
      } else {
        const entries = hashDraft.reduce<Record<string, string>>(
          (acc, row) => {
            if (row.field.trim()) {
              acc[row.field] = row.value;
            }
            return acc;
          },
          {},
        );
        await updateRedisKeyValue(
          connectionName,
          selectedKey,
          "hash",
          entries,
          db === "" ? undefined : db,
        );
      }
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="flex-1 overflow-hidden bg-background/50 px-6 pb-6 lg:px-8 lg:pb-8">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-2xl font-bold text-slate-100">Keys Browser</h2>
          <p className="text-sm text-slate-400">
            Explore keys for {connectionName} with advanced filters.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <button
            className="flex items-center gap-2 rounded-lg border border-border-dark bg-background px-4 py-2 text-sm font-semibold text-slate-100 transition-colors hover:border-primary"
            type="button"
          >
            <span className="material-symbols-outlined text-[18px]">add</span>
            Add Key
          </button>
          <button
            className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-4 py-2 text-sm font-semibold text-slate-100 transition-colors hover:border-primary"
            type="button"
          >
            <span className="material-symbols-outlined text-[18px]">info</span>
            Server Info
          </button>
          <button
            className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-2 text-sm font-medium transition-colors hover:border-primary disabled:cursor-not-allowed disabled:opacity-60"
            onClick={() => loadKeys()}
            type="button"
            disabled={isLoading}
          >
            {isLoading ? (
              <InlineSpinner className="size-4 border-slate-300" />
            ) : (
              <span className="material-symbols-outlined text-[18px]">
                refresh
              </span>
            )}
            {isLoading ? "Refreshing" : "Refresh"}
          </button>
        </div>
      </div>

      <div className="flex h-[calc(100vh-220px)] min-h-[560px] gap-4 overflow-hidden">
        <aside className="w-64 shrink-0 overflow-hidden rounded-xl border border-border-dark bg-surface-dark/40 p-4">
          <div className="mb-4">
            <p className="text-xs font-bold uppercase tracking-widest text-slate-500">
              Redis Instance
            </p>
            <div className="mt-2 flex items-center gap-2 text-sm text-slate-300">
              <span className="size-2 rounded-full bg-emerald-500"></span>
              <span className="truncate">{connectionName}</span>
            </div>
          </div>
          <div className="custom-scrollbar flex h-[calc(100%-110px)] flex-col gap-1 overflow-y-auto pr-2">
            {DB_OPTIONS.map((dbOption) => {
              const count = DB_COUNTS[dbOption] ?? 0;
              const isActive = db === dbOption;
              return (
                <button
                  key={dbOption}
                  className={`flex items-center justify-between rounded-lg px-3 py-2 text-sm transition-all ${
                    isActive
                      ? "border border-primary/40 bg-primary/10 text-primary"
                      : "text-slate-400 hover:bg-surface-dark"
                  }`}
                  type="button"
                  onClick={() => setDb(dbOption)}
                >
                  <div className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px]">
                      database
                    </span>
                    DB {dbOption}
                  </div>
                  <span className="rounded bg-background px-1.5 py-0.5 text-[10px] font-mono">
                    {count}
                  </span>
                </button>
              );
            })}
          </div>
          <button
            className="mt-4 flex w-full items-center justify-center gap-2 rounded-lg border border-rose-500/30 bg-rose-500/10 py-2 text-xs font-bold text-rose-400 transition-all hover:bg-rose-500 hover:text-white"
            type="button"
          >
            <span className="material-symbols-outlined text-[18px]">
              delete_forever
            </span>
            Flush DB {db === "" ? 0 : db}
          </button>
        </aside>

        <main className="flex flex-1 flex-col overflow-hidden rounded-xl border border-border-dark bg-surface-dark/30">
          <div className="border-b border-border-dark p-4">
            <div className="flex flex-col gap-3">
              <label className="flex flex-col gap-2">
                <span className="text-xs font-semibold uppercase text-slate-500">
                  Regex Search
                </span>
                <div className="flex h-10 items-center rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 focus-within:border-primary">
                  <span className="material-symbols-outlined text-[18px] text-slate-500">
                    search
                  </span>
                  <input
                    className="flex-1 bg-transparent px-3 text-sm font-mono text-slate-200 placeholder:text-slate-500 focus:ring-0"
                    placeholder="Search keys..."
                    value={pattern}
                    onChange={(event) => setPattern(event.target.value)}
                  />
                  <span className="rounded bg-background px-1.5 py-0.5 text-[10px] font-bold text-slate-400">
                    REGEX
                  </span>
                </div>
              </label>
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex flex-wrap gap-2">
                  {TYPE_FILTERS.map((type) => (
                    <button
                      key={type}
                      className={`h-7 rounded px-3 text-[11px] font-bold uppercase transition-all ${
                        filterType === type
                          ? "bg-primary text-white"
                          : "bg-background text-slate-400 hover:text-white"
                      }`}
                      type="button"
                      onClick={() => setFilterType(type)}
                    >
                      {type}
                    </button>
                  ))}
                </div>
                <button
                  className="flex items-center gap-1 rounded bg-primary px-3 py-1 text-[11px] font-bold uppercase text-white"
                  onClick={handleSearch}
                  type="button"
                >
                  <span className="material-symbols-outlined text-[14px]">
                    sync
                  </span>
                  Scan
                </button>
              </div>
            </div>
          </div>

          <AsyncGate
            isLoading={isLoading}
            error={error}
            empty={!isLoading && filteredKeys.length === 0}
          >
            <div className="flex flex-1 overflow-hidden">
              <div className="flex w-2/5 flex-col border-r border-border-dark">
                <div className="flex items-center border-b border-border-dark bg-surface-dark/50 px-4 py-2 text-[11px] font-bold uppercase tracking-wider text-slate-500">
                  <div className="w-1/2">Key Name</div>
                  <div className="w-1/6">Type</div>
                  <div className="w-1/6 text-right">TTL</div>
                  <div className="w-1/6 text-right">Size</div>
                </div>
                <div className="custom-scrollbar flex-1 overflow-y-auto">
                  {filteredKeys.map((key, index) => {
                    const info = keyInfoMap[key];
                    const type = info?.type ?? "unknown";
                    const ttl = formatTtl(info?.ttlSeconds);
                    const size =
                      key === selectedKey
                        ? formatSize(selectedValue?.value)
                        : "-";
                    const badgeStyle =
                      TYPE_BADGE_STYLES[type] ?? TYPE_BADGE_STYLES.unknown;
                    return (
                      <button
                        key={key}
                        className={`flex w-full items-center border-b border-border-dark/50 px-4 py-3 text-left text-sm transition-all ${
                          key === selectedKey || index === 0
                            ? "border-l-2 border-l-primary bg-primary/10 text-white"
                            : "text-slate-300 hover:bg-surface-dark/60"
                        }`}
                        type="button"
                        onClick={() => setSelectedKey(key)}
                      >
                        <div className="w-1/2 truncate font-mono">{key}</div>
                        <div className="w-1/6">
                          <span
                            className={`rounded border px-1.5 py-0.5 text-[10px] font-semibold uppercase ${badgeStyle}`}
                          >
                            {type}
                          </span>
                        </div>
                        <div className="w-1/6 text-right text-xs text-amber-400">
                          {ttl}
                        </div>
                        <div className="w-1/6 text-right text-xs text-slate-400">
                          {size}
                        </div>
                      </button>
                    );
                  })}
                </div>
                <div className="flex items-center justify-between border-t border-border-dark px-4 py-2 text-xs text-slate-400">
                  <span>{resultsLabel}</span>
                  <div className="flex items-center gap-2">
                    <button
                      className="rounded border border-border-dark px-2 py-1 disabled:cursor-not-allowed disabled:opacity-50"
                      type="button"
                      onClick={previousPage}
                      disabled={!hasPreviousPage}
                    >
                      Prev
                    </button>
                    <button
                      className="rounded border border-border-dark px-2 py-1 disabled:cursor-not-allowed disabled:opacity-50"
                      type="button"
                      onClick={nextPage}
                      disabled={!hasNextPage}
                    >
                      Next
                    </button>
                  </div>
                </div>
              </div>

              <div className="flex flex-1 flex-col">
                <div className="border-b border-border-dark p-6">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div>
                      <h3 className="text-xl font-bold text-slate-100">
                        {selectedKey ?? "Select a key"}
                      </h3>
                      <div className="mt-3 flex flex-wrap items-center gap-4 text-xs text-slate-400">
                        <div>
                          <span className="block uppercase tracking-widest">
                            Type
                          </span>
                          <span className="text-sm font-semibold text-emerald-400">
                            {selectedInfo?.type ?? "unknown"}
                          </span>
                        </div>
                        <div className="h-8 w-px bg-border-dark"></div>
                        <div>
                          <span className="block uppercase tracking-widest">
                            Memory
                          </span>
                          <span className="text-sm font-semibold">
                            {selectedValue?.value
                              ? formatSize(selectedValue.value)
                              : "-"}
                          </span>
                        </div>
                        <div className="h-8 w-px bg-border-dark"></div>
                        <div>
                          <span className="block uppercase tracking-widest">
                            Encoding
                          </span>
                          <span className="text-sm font-semibold font-mono">
                            -
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <button className="flex items-center gap-2 rounded-lg border border-border-dark bg-background px-4 py-2 text-sm font-semibold text-slate-100 transition-colors hover:border-primary">
                        <span className="material-symbols-outlined text-[18px]">
                          edit
                        </span>
                        Rename
                      </button>
                      <button className="flex items-center gap-2 rounded-lg border border-rose-500/40 bg-rose-500/10 px-4 py-2 text-sm font-semibold text-rose-300 transition-colors hover:bg-rose-500 hover:text-white">
                        <span className="material-symbols-outlined text-[18px]">
                          delete
                        </span>
                        Delete
                      </button>
                    </div>
                  </div>

                  <div className="mt-6 rounded-xl border border-border-dark bg-surface-dark/60 p-4">
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <div className="flex items-center gap-2 text-sm font-semibold text-slate-200">
                        <span className="material-symbols-outlined text-[18px] text-amber-400">
                          timer
                        </span>
                        Time To Live (TTL)
                      </div>
                      <div className="flex items-center gap-2 text-sm text-slate-400">
                        <input
                          className="h-8 w-16 rounded border border-border-dark bg-background text-center text-sm font-mono text-slate-100"
                          type="text"
                          value={selectedInfo?.ttlSeconds ?? ""}
                          readOnly
                        />
                        seconds
                        <button className="rounded bg-primary/20 px-3 py-1 text-xs font-bold text-primary transition-all hover:bg-primary hover:text-white">
                          Update
                        </button>
                      </div>
                    </div>
                    <input
                      className="mt-3 h-1.5 w-full cursor-pointer accent-primary"
                      type="range"
                      min={0}
                      max={100}
                      value={34}
                      readOnly
                    />
                  </div>
                </div>

                <div className="flex items-center justify-between border-b border-border-dark bg-surface-dark/50 px-6 py-3">
                  <div className="flex flex-wrap items-center gap-3 text-xs font-bold uppercase tracking-widest text-slate-400">
                    <span>Values</span>
                    <div className="flex items-center gap-1 rounded-lg border border-border-dark bg-background p-0.5">
                      <button
                        className={`rounded px-3 py-1 text-[10px] font-bold uppercase transition-colors ${
                          valueView === "table"
                            ? "bg-primary text-white"
                            : "text-slate-400 hover:text-white"
                        }`}
                        type="button"
                        onClick={() => setValueView("table")}
                      >
                        Table
                      </button>
                      <button
                        className={`rounded px-3 py-1 text-[10px] font-bold uppercase transition-colors ${
                          valueView === "raw"
                            ? "bg-primary text-white"
                            : "text-slate-400 hover:text-white"
                        }`}
                        type="button"
                        onClick={() => setValueView("raw")}
                      >
                        Raw
                      </button>
                    </div>
                    {valueView === "table" && (
                      <div className="flex items-center gap-2 rounded border border-border-dark bg-background px-2 py-1 text-[11px] text-slate-400">
                        <span className="material-symbols-outlined text-[14px]">
                          search
                        </span>
                        <input
                          className="w-32 bg-transparent text-[11px] text-slate-300 focus:ring-0"
                          placeholder="Filter fields..."
                        />
                      </div>
                    )}
                  </div>
                  <button
                    className="flex items-center gap-1 rounded bg-primary px-3 py-1 text-[11px] font-bold uppercase text-white disabled:cursor-not-allowed disabled:opacity-60"
                    type="button"
                    disabled={
                      isSaving ||
                      !selectedInfo ||
                      (selectedInfo.type !== "string" &&
                        selectedInfo.type !== "hash")
                    }
                    onClick={handleSave}
                  >
                    <span className="material-symbols-outlined text-[14px]">
                      {isSaving ? "hourglass_top" : "save"}
                    </span>
                    {isSaving ? "Saving" : "Save Changes"}
                  </button>
                </div>

                <div className="custom-scrollbar flex-1 overflow-auto">
                  {valueView === "table" ? (
                    <table className="w-full text-left text-sm">
                      <thead className="sticky top-0 border-b border-border-dark bg-background text-[10px] font-bold uppercase tracking-wider text-slate-500">
                        <tr>
                          <th className="px-6 py-3 w-1/3">Field</th>
                          <th className="px-6 py-3">Value</th>
                          <th className="px-6 py-3 w-16"></th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-border-dark">
                        {selectedInfo?.type === "hash" &&
                          hashDraft.map((row, index) => (
                            <tr
                              key={`${row.field}-${index}`}
                              className="group transition-colors hover:bg-surface-dark/60"
                            >
                              <td className="px-6 py-3">
                                <input
                                  className="w-full bg-transparent font-mono text-sm text-primary focus:outline-none"
                                  value={row.field}
                                  onChange={(event) =>
                                    handleHashChange(
                                      index,
                                      "field",
                                      event.target.value,
                                    )
                                  }
                                />
                              </td>
                              <td className="px-6 py-3">
                                <input
                                  className="w-full bg-transparent font-mono text-sm text-slate-200 focus:outline-none"
                                  value={row.value}
                                  onChange={(event) =>
                                    handleHashChange(
                                      index,
                                      "value",
                                      event.target.value,
                                    )
                                  }
                                />
                              </td>
                              <td className="px-6 py-3 text-right">
                                <button
                                  className="opacity-0 transition-opacity group-hover:opacity-100"
                                  type="button"
                                  onClick={() => handleHashRemove(index)}
                                >
                                  <span className="material-symbols-outlined text-[16px] text-slate-500 hover:text-rose-400">
                                    delete
                                  </span>
                                </button>
                              </td>
                            </tr>
                          ))}
                        {selectedInfo?.type === "hash" && (
                          <tr>
                            <td className="px-6 py-4" colSpan={3}>
                              <button
                                className="flex items-center gap-1 text-xs font-bold text-primary hover:underline"
                                type="button"
                                onClick={handleAddHashRow}
                              >
                                <span className="material-symbols-outlined text-[16px]">
                                  add_circle
                                </span>
                                Add new field
                              </button>
                            </td>
                          </tr>
                        )}
                        {selectedInfo?.type !== "hash" && (
                          <tr>
                            <td className="px-6 py-4" colSpan={3}>
                              <pre className="whitespace-pre-wrap rounded-lg border border-border-dark bg-background/40 p-3 text-xs text-slate-200">
                                {selectedValue
                                  ? JSON.stringify(selectedValue.value, null, 2)
                                  : "Select a key to view its value."}
                              </pre>
                            </td>
                          </tr>
                        )}
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
                              setContentFormat(
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
                            prettify
                              ? "bg-primary/20 text-primary"
                              : "text-slate-400"
                          }`}
                          type="button"
                          onClick={() => setPrettify((prev) => !prev)}
                        >
                          Prettify
                        </button>
                      </div>
                      <textarea
                        className="min-h-[240px] w-full rounded-lg border border-border-dark bg-background/40 p-4 font-mono text-xs text-slate-200 focus:ring-2 focus:ring-primary/50"
                        value={rawDraft}
                        onChange={(event) => setRawDraft(event.target.value)}
                      />
                      <div className="mt-3 text-xs text-slate-400">
                        Preview:
                        <pre className="mt-2 whitespace-pre-wrap rounded-lg border border-border-dark bg-background/40 p-3 text-xs text-slate-200">
                          {formatRawValue()}
                        </pre>
                      </div>
                    </div>
                  )}
                </div>

                <footer className="flex items-center justify-between border-t border-border-dark bg-background/60 px-4 py-2 text-[10px] text-slate-400">
                  <div className="flex items-center gap-3">
                    <span className="flex items-center gap-1.5">
                      <span className="size-1.5 rounded-full bg-emerald-500"></span>
                      redis_version: 7.0.12
                    </span>
                    <span className="uppercase tracking-widest opacity-60">
                      Latency: 12ms
                    </span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="font-bold uppercase tracking-widest text-primary">
                      UTF-8
                    </span>
                    <span>Line 1, Col 45</span>
                  </div>
                </footer>
              </div>
            </div>
          </AsyncGate>
        </main>
      </div>
    </div>
  );
}
