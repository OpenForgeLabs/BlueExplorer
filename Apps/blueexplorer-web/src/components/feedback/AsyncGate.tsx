type AsyncGateProps = {
  isLoading: boolean;
  error?: string;
  empty?: boolean;
  loadingFallback?: React.ReactNode;
  errorFallback?: React.ReactNode;
  emptyFallback?: React.ReactNode;
  children: React.ReactNode;
};

export function AsyncGate({
  isLoading,
  error,
  empty,
  loadingFallback,
  errorFallback,
  emptyFallback,
  children,
}: AsyncGateProps) {
  if (isLoading) {
    return (
      loadingFallback ?? (
        <div className="rounded-xl border border-dashed border-border-dark p-8 text-center text-sm text-slate-400">
          Loading...
        </div>
      )
    );
  }

  if (error) {
    return (
      errorFallback ?? (
        <div className="rounded-xl border border-dashed border-rose-500/40 bg-rose-500/10 p-8 text-center text-sm text-rose-200">
          {error}
        </div>
      )
    );
  }

  if (empty) {
    return (
      emptyFallback ?? (
        <div className="rounded-xl border border-dashed border-border-dark p-8 text-center text-sm text-slate-400">
          No data available.
        </div>
      )
    );
  }

  return <>{children}</>;
}
