"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { AsyncGate } from "@/components/feedback/AsyncGate";
import { RedisKeyValueEditor } from "@/features/redis/keys/components/editors/RedisKeyValueEditor";
import { RedisValueEditorHandle } from "@/features/redis/keys/components/editors/RedisValueEditorTypes";
import { RedisDbSidebar } from "@/features/redis/keys/components/layout/RedisDbSidebar";
import { AddKeyModal } from "@/features/redis/keys/components/layout/AddKeyModal";
import { RedisServerInfoModal } from "@/features/redis/keys/components/layout/RedisServerInfoModal";
import { RedisKeysFooter } from "@/features/redis/keys/components/layout/RedisKeysFooter";
import { RedisKeyHeader } from "@/features/redis/keys/components/layout/RedisKeyHeader";
import { RedisKeysHeader } from "@/features/redis/keys/components/layout/RedisKeysHeader";
import { RedisKeysFilters } from "@/features/redis/keys/components/layout/RedisKeysFilters";
import { RedisKeysList } from "@/features/redis/keys/components/layout/RedisKeysList";
import { ConfirmActionModal } from "@/components/modals/ConfirmActionModal";
import { useRedisKeyActions } from "@/features/redis/keys/hooks/useRedisKeyActions";
import { useRedisKeys } from "@/features/redis/keys/hooks/useRedisKeys";
import { fetchRedisDbSize } from "@/features/redis/keys/services/redisDbSizeService";
import { RedisKeyType } from "@/lib/types";

const DB_OPTIONS = Array.from({ length: 16 }, (_, idx) => idx);
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

const TYPE_DESCRIPTIONS: Record<RedisKeyType, string> = {
  string: "Binary-safe string values, ideal for counters or cached payloads.",
  hash: "Field-value maps that resemble objects or structured records.",
  list: "Ordered collections useful for queues and timelines.",
  set: "Unordered unique members for tags or membership tracking.",
  zset: "Sorted sets with scores for rankings and leaderboards.",
  stream: "Append-only event logs for messaging pipelines.",
  unknown: "Unsupported or custom module type.",
};

type RedisKeysScreenProps = {
  connectionName: string;
};

export function RedisKeysScreen({ connectionName }: RedisKeysScreenProps) {
  const [pattern, setPattern] = useState("");
  const [db, setDb] = useState<number | "">(0);
  const [filterType, setFilterType] = useState<"all" | RedisKeyType>("all");
  const [selectedKey, setSelectedKey] = useState<string | null>(null);
  const [dbCounts, setDbCounts] = useState<
    Record<number, number | null | undefined>
  >({});
  const [dbCountsLoading, setDbCountsLoading] = useState<Set<number>>(
    () => new Set(),
  );
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [flushModalOpen, setFlushModalOpen] = useState(false);
  const [serverInfoOpen, setServerInfoOpen] = useState(false);
  const [addKeyOpen, setAddKeyOpen] = useState(false);
  const editorRef = useRef<RedisValueEditorHandle>(null);

  const {
    data,
    error,
    isLoading,
    loadKeys,
    keyInfoMap,
    valueMap,
    loadKeyValue,
    refreshKeyValue,
    refreshKeyInfo,
    refreshKeyData,
    nextPage,
    previousPage,
    hasNextPage,
    hasPreviousPage,
  } = useRedisKeys(connectionName);

  const loadKeysRef = useRef(loadKeys);
  useEffect(() => {
    loadKeysRef.current = loadKeys;
  }, [loadKeys]);
  const refreshKeys = useCallback(() => {
    loadKeysRef.current(
      {
        pattern: pattern || undefined,
        db: db === "" ? undefined : db,
        cursor: 0,
      },
      true,
    );
    if (db !== "") {
      setDbCounts((previous) => ({
        ...previous,
        [db]: undefined,
      }));
    }
  }, [db, pattern]);

  const selectedInfoFromBackend = selectedKey
    ? keyInfoMap[selectedKey]
    : undefined;

  const {
    isSaving,
    isRenaming,
    nameDraft,
    ttlDraft,
    hasEditorErrors,
    saveError,
    ttlError,
    localKeys,
    localKeyInfo,
    setIsRenaming,
    setNameDraft,
    setTtlDraft,
    handleAddKey,
    handleNewKeyTypeChange,
    handleRenameConfirm,
    handleSave,
    handleDeleteKey,
    handleFlushDb,
  } = useRedisKeyActions({
    connectionName,
    db,
    selectedKey,
    selectedInfo: selectedInfoFromBackend,
    editorRef,
    onSelectKey: setSelectedKey,
    onRefreshKeys: refreshKeys,
    onRefreshKeyValue: refreshKeyValue,
    onRefreshKeyInfo: refreshKeyInfo,
    onRefreshKeyData: refreshKeyData,
  });

  const handleSearch = () => {
    refreshKeys();
  };

  const loadDbCount = useCallback(
    async (dbIndex: number, force = false) => {
      if (!force && dbCounts[dbIndex] !== undefined) {
        return;
      }
      setDbCountsLoading((previous) => {
        const next = new Set(previous);
        next.add(dbIndex);
        return next;
      });
      try {
        const response = await fetchRedisDbSize(connectionName, dbIndex);
        setDbCounts((previous) => ({
          ...previous,
          [dbIndex]: response.isSuccess ? response.data ?? 0 : null,
        }));
      } finally {
        setDbCountsLoading((previous) => {
          const next = new Set(previous);
          next.delete(dbIndex);
          return next;
        });
      }
    },
    [connectionName, dbCounts],
  );

  useEffect(() => {
    if (db === "") {
      return;
    }
    refreshKeys();
  }, [db, pattern, refreshKeys]);

  useEffect(() => {
    setDbCounts({});
    setDbCountsLoading(new Set());
  }, [connectionName]);

  useEffect(() => {
    if (db === "") {
      return;
    }
    if (dbCounts[db] === undefined) {
      void loadDbCount(db, true);
    }
  }, [db, dbCounts, loadDbCount]);

  const combinedKeyInfoMap = useMemo(
    () => ({ ...keyInfoMap, ...localKeyInfo }),
    [keyInfoMap, localKeyInfo],
  );

  const allKeys = useMemo(() => {
    const merged = [...localKeys, ...data.keys];
    return Array.from(new Set(merged));
  }, [localKeys, data.keys]);

  const filteredKeys = useMemo(() => {
    if (filterType === "all") {
      return allKeys;
    }
    return allKeys.filter(
      (key) => combinedKeyInfoMap[key]?.type === filterType,
    );
  }, [allKeys, filterType, combinedKeyInfoMap]);

  const isLocalKey = useMemo(() => {
    if (!selectedKey) {
      return false;
    }
    return localKeys.includes(selectedKey);
  }, [selectedKey, localKeys]);

  useEffect(() => {
    if (!filteredKeys.length) {
      const timeout = setTimeout(() => setSelectedKey(null), 0);
      return () => clearTimeout(timeout);
    }
    if (!selectedKey || !filteredKeys.includes(selectedKey)) {
      const timeout = setTimeout(() => setSelectedKey(filteredKeys[0]), 0);
      return () => clearTimeout(timeout);
    }
    return undefined;
  }, [filteredKeys, selectedKey]);

  useEffect(() => {
    if (!selectedKey) {
      return;
    }
    const info = combinedKeyInfoMap[selectedKey];
    if (!info) {
      return;
    }
    loadKeyValue(selectedKey, info.type, db === "" ? undefined : db);
  }, [selectedKey, combinedKeyInfoMap, db, loadKeyValue]);

  const selectedInfoRaw = selectedKey
    ? combinedKeyInfoMap[selectedKey]
    : undefined;
  const selectedInfo = selectedKey && selectedInfoRaw
    ? { key: selectedKey, ...selectedInfoRaw }
    : undefined;
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

  const selectedType = selectedInfo?.type ?? "unknown";

  return (
    <div className="flex h-full min-h-0 flex-1 flex-col overflow-hidden bg-background/50 px-4 pb-6 sm:px-6 lg:px-8 lg:pb-8">
      <RedisKeysHeader
        connectionName={connectionName}
        isLoading={isLoading}
        onRefresh={refreshKeys}
        onServerInfo={() => setServerInfoOpen(true)}
      />

      <div className="flex max-h-[calc(100dvh-160px)] min-h-0 flex-1 flex-col gap-4 overflow-hidden lg:flex-row">
        <RedisDbSidebar
          connectionName={connectionName}
          db={db}
          dbOptions={DB_OPTIONS}
          dbCounts={dbCounts}
          dbCountsLoading={dbCountsLoading}
          onSelectDb={(dbOption) => {
            setDb(dbOption);
            void loadDbCount(dbOption);
          }}
          onFlushDb={() => setFlushModalOpen(true)}
        />

        <main className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-border-dark bg-surface-dark/30">
          <RedisKeysFilters
            pattern={pattern}
            filterType={filterType}
            isLoading={isLoading}
            typeFilters={TYPE_FILTERS}
            onPatternChange={setPattern}
            onFilterChange={setFilterType}
            onSearch={handleSearch}
            onAddKey={() => setAddKeyOpen(true)}
          />

          <AsyncGate
            isLoading={isLoading}
            error={error}
            empty={!isLoading && filteredKeys.length === 0}
          >
            <div className="flex min-h-0 flex-1 flex-col overflow-hidden lg:flex-row">
              <RedisKeysList
                keys={filteredKeys}
                selectedKey={selectedKey}
                keyInfoMap={combinedKeyInfoMap}
                selectedValue={selectedValue}
                localKeys={localKeys}
                resultsLabel={resultsLabel}
                hasNextPage={hasNextPage}
                hasPreviousPage={hasPreviousPage}
                formatTtl={formatTtl}
                formatSize={formatSize}
                onSelectKey={setSelectedKey}
                onNextPage={nextPage}
                onPreviousPage={previousPage}
                typeBadgeStyles={TYPE_BADGE_STYLES}
              />

              <div className="flex min-h-0 flex-1 flex-col">
                <div className="flex min-h-0 flex-1 flex-col">
                  <RedisKeyHeader
                    selectedKey={selectedKey}
                    selectedType={selectedType}
                    selectedValue={selectedValue?.value}
                    nameDraft={nameDraft}
                    isRenaming={isRenaming}
                    isSaving={isSaving}
                    canSave={
                      !!selectedInfo &&
                      selectedInfo.type !== "unknown" &&
                      !hasEditorErrors
                    }
                    ttlValue={ttlDraft}
                    ttlError={ttlError}
                    saveError={saveError}
                    typeDescription={TYPE_DESCRIPTIONS[selectedType]}
                    isLocalKey={isLocalKey}
                    onRenameToggle={() => setIsRenaming(true)}
                    onRenameConfirm={handleRenameConfirm}
                    onNameChange={setNameDraft}
                    onTtlChange={setTtlDraft}
                    onRefreshValue={() => {
                      if (selectedKey && selectedInfo) {
                        refreshKeyValue(
                          selectedKey,
                          selectedInfo.type,
                          db === "" ? undefined : db,
                        );
                      }
                    }}
                    onSave={handleSave}
                    onDelete={() => setDeleteModalOpen(true)}
                    onTypeChange={handleNewKeyTypeChange}
                  />

                  <div className="flex min-h-0 flex-1 overflow-auto">
                    {selectedInfo ? (
                      <RedisKeyValueEditor
                        key={`${selectedKey ?? "key"}-${selectedType}`}
                        ref={editorRef}
                        type={selectedType}
                        value={selectedValue?.value}
                      />
                    ) : (
                      <div className="flex flex-1 items-center justify-center text-sm text-slate-500">
                        Select a key to view its value.
                      </div>
                    )}
                  </div>
                </div>

                <RedisKeysFooter connectionName={connectionName} />
              </div>
            </div>
          </AsyncGate>
        </main>
      </div>

      <ConfirmActionModal
        open={deleteModalOpen}
        title="Delete key"
        description="This will permanently delete the selected key."
        confirmLabel="Delete key"
        confirmValue={selectedKey ?? ""}
        confirmPlaceholder="Type key name"
        onCancel={() => setDeleteModalOpen(false)}
        onConfirm={async () => {
          if (selectedKey) {
            await handleDeleteKey(selectedKey);
          }
          setDeleteModalOpen(false);
        }}
      />

      <ConfirmActionModal
        open={flushModalOpen}
        title="Flush database"
        description="This will permanently delete all keys in the selected database."
        confirmLabel="Flush DB"
        confirmValue={db === "" ? "0" : String(db)}
        confirmPlaceholder="Type DB index"
        onCancel={() => setFlushModalOpen(false)}
        onConfirm={async () => {
          const dbIndex = db === "" ? 0 : db;
          await handleFlushDb(dbIndex, String(dbIndex));
          setFlushModalOpen(false);
        }}
      />

      <RedisServerInfoModal
        open={serverInfoOpen}
        connectionName={connectionName}
        onClose={() => setServerInfoOpen(false)}
      />
      <AddKeyModal
        open={addKeyOpen}
        onCancel={() => setAddKeyOpen(false)}
        defaultType={filterType === "all" ? undefined : filterType}
        onConfirm={(name, type) => {
          handleAddKey(name, type);
          setAddKeyOpen(false);
        }}
      />
    </div>
  );
}
