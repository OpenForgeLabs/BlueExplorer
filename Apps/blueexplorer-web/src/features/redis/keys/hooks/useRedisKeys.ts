import { useEffect, useState } from "react";
import { useAsyncAction } from "@/lib/async/useAsyncAction";
import { RedisKeyInfo, RedisKeyScanResult, RedisKeyType, RedisKeyValue } from "@/lib/types";
import { fetchRedisKeys, RedisKeysQuery } from "@/features/redis/keys/services/redisKeysService";
import { fetchRedisKeyInfo } from "@/features/redis/keys/services/redisKeyInfoService";
import { fetchRedisKeyValue } from "@/features/redis/keys/services/redisKeyValueService";

type RedisKeysState = {
  data: RedisKeyScanResult;
  error?: string;
};

const DEFAULT_RESULT: RedisKeyScanResult = { keys: [], cursor: 0 };

export function useRedisKeys(connectionName: string) {
  const [state, setState] = useState<RedisKeysState>({ data: DEFAULT_RESULT });
  const [params, setParams] = useState<Omit<RedisKeysQuery, "connectionName">>({
    pageSize: 100,
    cursor: 0,
  });
  const [cursorHistory, setCursorHistory] = useState<number[]>([]);
  const [keyInfoMap, setKeyInfoMap] = useState<Record<string, RedisKeyInfo>>({});
  const [valueMap, setValueMap] = useState<Record<string, RedisKeyValue>>({});

  const { run, isLoading, error } = useAsyncAction(fetchRedisKeys, {
    label: "Loading Redis keys",
  });

  const loadKeys = async (
    next?: Partial<Omit<RedisKeysQuery, "connectionName">>,
    resetHistory = false,
  ) => {
    const merged = { ...params, ...next };
    setParams(merged);
    if (resetHistory) {
      setCursorHistory([]);
    }
    try {
      const response = await run({ connectionName, ...merged });
      if (!response.isSuccess) {
        setState({
          data: response.data ?? DEFAULT_RESULT,
          error: response.message || "Unable to load keys",
        });
        return;
      }
      setState({ data: response.data ?? DEFAULT_RESULT, error: undefined });
    } catch {
      setState({ data: DEFAULT_RESULT, error: "Unable to load keys" });
    }
  };

  useEffect(() => {
    loadKeys(undefined, true);
  }, [connectionName]);

  const loadKeyInfos = async (keys: string[], db?: number) => {
    const missingKeys = keys.filter((key) => !keyInfoMap[key]);
    if (!missingKeys.length) {
      return;
    }
    const results = await Promise.all(
      missingKeys.map((key) => fetchRedisKeyInfo(connectionName, key, db)),
    );
    setKeyInfoMap((previous) => {
      const nextMap = { ...previous };
      results.forEach((result, index) => {
        if (result.isSuccess && result.data) {
          nextMap[missingKeys[index]] = result.data;
        }
      });
      return nextMap;
    });
  };

  useEffect(() => {
    if (!state.data.keys.length) {
      return;
    }
    loadKeyInfos(state.data.keys, params.db);
  }, [state.data.keys, params.db]);

  const loadKeyValue = async (key: string, type: RedisKeyType, db?: number) => {
    if (valueMap[key] && valueMap[key].type === type) {
      return;
    }
    const response = await fetchRedisKeyValue(connectionName, key, type, db);
    if (response.isSuccess && response.data) {
      setValueMap((previous) => ({ ...previous, [key]: response.data }));
    }
  };

  const nextPage = async () => {
    if (!state.data.cursor) {
      return;
    }
    setCursorHistory((previous) => [...previous, params.cursor ?? 0]);
    await loadKeys({ cursor: state.data.cursor });
  };

  const previousPage = async () => {
    if (!cursorHistory.length) {
      return;
    }
    const previousCursor = cursorHistory[cursorHistory.length - 1];
    setCursorHistory((previous) => previous.slice(0, -1));
    await loadKeys({ cursor: previousCursor });
  };

  const hasNextPage = state.data.cursor !== 0;
  const hasPreviousPage = cursorHistory.length > 0;

  return {
    data: state.data,
    error: state.error ?? error,
    isLoading,
    params,
    loadKeys,
    loadKeyValue,
    keyInfoMap,
    valueMap,
    nextPage,
    previousPage,
    hasNextPage,
    hasPreviousPage,
  };
}
