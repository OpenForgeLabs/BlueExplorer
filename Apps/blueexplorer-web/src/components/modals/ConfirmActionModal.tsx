"use client";

import { useMemo, useState } from "react";
import { Modal } from "@/components/Modal";

type ConfirmActionModalProps = {
  open: boolean;
  title: string;
  description: string;
  confirmLabel: string;
  confirmValue: string;
  confirmPlaceholder?: string;
  onCancel: () => void;
  onConfirm: () => Promise<void> | void;
};

export function ConfirmActionModal({
  open,
  title,
  description,
  confirmLabel,
  confirmValue,
  confirmPlaceholder,
  onCancel,
  onConfirm,
}: ConfirmActionModalProps) {
  const [inputValue, setInputValue] = useState("");

  const canConfirm = useMemo(
    () => inputValue.trim() === confirmValue,
    [inputValue, confirmValue],
  );

  return (
    <Modal
      open={open}
      title={title}
      description={description}
      footer={
        <div className="flex flex-wrap justify-end gap-3">
          <button
            className="rounded-md border border-border-dark bg-transparent px-4 py-2 text-sm text-slate-300 hover:border-slate-500"
            type="button"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="rounded-md bg-rose-500 px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            disabled={!canConfirm}
            onClick={async () => {
              await onConfirm();
              setInputValue("");
            }}
          >
            {confirmLabel}
          </button>
        </div>
      }
    >
      <div className="space-y-4">
        <p className="text-sm text-slate-400">{description}</p>
        <div>
          <label className="text-xs font-semibold uppercase tracking-widest text-slate-500">
            Type <span className="text-rose-400">{confirmValue}</span> to
            confirm
          </label>
          <input
            className="mt-2 w-full rounded-md border border-border-dark bg-background px-3 py-2 text-sm text-slate-100"
            placeholder={confirmPlaceholder ?? confirmValue}
            value={inputValue}
            onChange={(event) => setInputValue(event.target.value)}
          />
        </div>
      </div>
    </Modal>
  );
}
