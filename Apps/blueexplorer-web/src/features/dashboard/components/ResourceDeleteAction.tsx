"use client";

import { useState } from "react";
import { ConfirmActionModal } from "@/components/modals/ConfirmActionModal";
import { ResourceSummaryBase } from "@/lib/types";

type ResourceDeleteActionProps = {
  resource: ResourceSummaryBase;
  onDelete: (resource: ResourceSummaryBase) => Promise<void>;
};

export function ResourceDeleteAction({
  resource,
  onDelete,
}: ResourceDeleteActionProps) {
  const [open, setOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  return (
    <>
      <button
        className="flex items-center gap-1 rounded-md border border-border-dark bg-surface-dark/60 px-2 py-1 text-[11px] font-medium text-slate-300 transition-colors hover:border-rose-500/60 hover:text-rose-200"
        type="button"
        onClick={(event) => {
          event.preventDefault();
          event.stopPropagation();
          setError(null);
          setOpen(true);
        }}
        aria-label={`Delete ${resource.name}`}
      >
        <span className="material-symbols-outlined text-[16px]">
          delete
        </span>
      </button>
      <ConfirmActionModal
        open={open}
        title={`Delete ${resource.type === "redis" ? "Redis" : "Service Bus"} connection`}
        description={`This will remove the saved connection "${resource.name}" from BlueExplorer. This does not delete any Azure resource.`}
        confirmLabel={isDeleting ? "Deleting..." : "Delete"}
        confirmValue={resource.name}
        onCancel={() => setOpen(false)}
        onConfirm={async () => {
          if (isDeleting) {
            return;
          }
          setIsDeleting(true);
          setError(null);
          try {
            await onDelete(resource);
            setOpen(false);
          } catch (err) {
            setError(
              err instanceof Error ? err.message : "Failed to delete connection.",
            );
          } finally {
            setIsDeleting(false);
          }
        }}
      />
      {open && error ? (
        <div className="mt-2 text-xs text-rose-300">{error}</div>
      ) : null}
    </>
  );
}
