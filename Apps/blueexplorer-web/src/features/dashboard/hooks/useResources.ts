import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAsyncAction } from "@/lib/async/useAsyncAction";
import { ResourceSummary, ResourceType } from "@/lib/types";
import { fetchAllResources } from "@/features/dashboard/services/resourcesService";

type ResourcesState = {
  data: ResourceSummary[];
  error?: string;
};

export function useResources(type: ResourceType) {
  const [allResources, setAllResources] = useState<ResourceSummary[]>([]);
  const [state, setState] = useState<ResourcesState>({ data: [] });
  const { run, isLoading, error } = useAsyncAction(fetchAllResources, {
    label: "Refreshing resources",
  });
  const runRef = useRef(run);

  useEffect(() => {
    runRef.current = run;
  }, [run]);

  const refresh = useCallback(async () => {
    try {
      const response = await runRef.current();

      if (!response.isSuccess) {
        setState({
          data: response.data ?? [],
          error: response.message || "Unable to load resources",
        });
        return;
      }

      setAllResources(response.data ?? []);
      setState({
        data: response.data ?? [],
        error: undefined,
      });
    } catch {
      setState({
        data: [],
        error: "Unable to load resources",
      });
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, []);

  useEffect(() => {
    const handler = () => {
      void refresh();
    };
    window.addEventListener("resources:refresh", handler);
    return () => window.removeEventListener("resources:refresh", handler);
  }, [refresh]);

  const filteredResources = useMemo(() => {
    if (type === "all") {
      return allResources;
    }
    return allResources.filter((resource) => resource.type === type);
  }, [allResources, type]);

  return {
    data: filteredResources,
    allResources,
    isLoading,
    error: state.error ?? error,
    refresh,
  };
}
