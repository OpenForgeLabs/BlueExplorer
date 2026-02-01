import { useEffect, useState } from "react";
import { useAsyncAction } from "@/lib/async/useAsyncAction";
import { ResourceSummary, ResourceType } from "@/lib/types";
import { fetchAllResources } from "@/features/dashboard/services/resourcesService";

type ResourcesState = {
  data: ResourceSummary[];
  error?: string;
};

export function useResources(type: ResourceType) {
  const [state, setState] = useState<ResourcesState>({ data: [] });

  const [allResources, setAllResources] = useState<ResourceSummary[]>([]);
  const { run, isLoading, error } = useAsyncAction(fetchAllResources, {
    label: "Refreshing resources",
  });

  const refresh = async () => {
    try {
      const response = await run();

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
  };

  useEffect(() => {
    refresh();
  }, []);

  useEffect(() => {
    if (!allResources.length) {
      return;
    }

    if (type === "all") {
      setState((previous) => ({ ...previous, data: allResources }));
      return;
    }

    setState((previous) => ({
      ...previous,
      data: allResources.filter((resource) => resource.type === type),
    }));
  }, [type, allResources]);

  return {
    ...state,
    allResources,
    isLoading,
    error: state.error ?? error,
    refresh,
  };
}
