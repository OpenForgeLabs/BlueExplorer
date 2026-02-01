type MetricStatProps = {
  label: string;
  value: string | number;
  className?: string;
};

export function MetricStat({ label, value, className }: MetricStatProps) {
  return (
    <div
      className={`rounded-lg border border-border-dark/70 bg-background/40 p-2 ${className ?? ""}`}
    >
      <p className="mb-1 text-[10px] font-bold uppercase tracking-tighter text-slate-500">
        {label}
      </p>
      <p className="text-sm font-semibold text-slate-100">{value}</p>
    </div>
  );
}
