import { InlineSpinner } from "@/components/feedback/InlineSpinner";

type LoadingOverlayProps = {
  label?: string;
};

export function LoadingOverlay({ label = "Loading..." }: LoadingOverlayProps) {
  return (
    <div className="absolute inset-0 z-20 flex items-center justify-center bg-slate-900/70">
      <div className="flex items-center gap-3 rounded-lg border border-border-dark bg-background px-4 py-3 text-sm text-slate-100 shadow-lg">
        <InlineSpinner />
        {label}
      </div>
    </div>
  );
}
