export function InlineSpinner({ className }: { className?: string }) {
  return (
    <span
      className={`inline-block size-4 animate-spin rounded-full border-2 border-slate-400 border-t-transparent ${className ?? ""}`}
    />
  );
}
