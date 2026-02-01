import { useEffect, useState } from "react";
import { useAsyncAction } from "@/lib/async/useAsyncAction";
import { RedisServerStats } from "@/lib/types";
import { fetchRedisStats } from "@/features/redis/overview/services/redisOverviewService";

type RedisOverviewState = {
  data: RedisServerStats;
  error?: string;
};

export function useRedisOverview(connectionName: string) {
  const [state, setState] = useState<RedisOverviewState>({ data: {} });
  const { run, isLoading, error } = useAsyncAction(fetchRedisStats, {
    label: "Loading Redis stats",
  });

  const refresh = async () => {
    try {
      const response = await run(connectionName);
      if (!response.isSuccess) {
        setState({
          data: response.data ?? {},
          error: response.message || "Unable to load stats",
        });
        return;
      }
      setState({ data: response.data ?? {}, error: undefined });
    } catch {
      setState({ data: {}, error: "Unable to load stats" });
    }
  };

  useEffect(() => {
    refresh();
  }, [connectionName]);

  return { data: state.data, error: state.error ?? error, isLoading, refresh };
}
