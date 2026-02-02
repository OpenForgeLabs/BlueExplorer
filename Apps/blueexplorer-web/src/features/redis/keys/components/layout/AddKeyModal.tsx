import { useEffect, useMemo, useState } from "react";
import { Modal } from "@/components/Modal";
import { RedisKeyType } from "@/lib/types";

type AddKeyModalProps = {
  open: boolean;
  onCancel: () => void;
  onConfirm: (name: string, type: RedisKeyType) => void;
  defaultType?: RedisKeyType;
};

const KEY_TYPES: RedisKeyType[] = [
  "string",
  "hash",
  "list",
  "set",
  "zset",
  "stream",
];

export function AddKeyModal({
  open,
  onCancel,
  onConfirm,
  defaultType,
}: AddKeyModalProps) {
  const [name, setName] = useState("");
  const [type, setType] = useState<RedisKeyType>("string");

  const trimmedName = useMemo(() => name.trim(), [name]);
  const canSubmit = trimmedName.length > 0;

  useEffect(() => {
    if (!open) {
      return;
    }
    setName("");
    setType(defaultType ?? "string");
  }, [open, defaultType]);

  const handleConfirm = () => {
    if (!canSubmit) {
      return;
    }
    onConfirm(trimmedName, type);
    setName("");
    setType(defaultType ?? "string");
  };

  return (
    <Modal
      open={open}
      title="Add key"
      description="Create a new Redis key and choose its type."
      footer={
        <div className="flex justify-end gap-2">
          <button
            className="rounded-md border border-border-dark bg-background px-4 py-2 text-sm text-slate-200 hover:border-primary"
            type="button"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-60"
            type="button"
            onClick={handleConfirm}
            disabled={!canSubmit}
          >
            Create
          </button>
        </div>
      }
    >
      <div className="flex flex-col gap-4">
        <label className="flex flex-col gap-2 text-sm text-slate-200">
          Key name
          <input
            className="rounded-md border border-border-dark bg-background px-3 py-2 text-sm text-slate-100 focus:ring-2 focus:ring-primary/40"
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder="e.g. cache:product:1"
          />
        </label>
        <label className="flex flex-col gap-2 text-sm text-slate-200">
          Key type
          <select
            className="rounded-md border border-border-dark bg-background px-3 py-2 text-sm text-slate-100"
            value={type}
            onChange={(event) => setType(event.target.value as RedisKeyType)}
          >
            {KEY_TYPES.map((keyType) => (
              <option key={keyType} value={keyType}>
                {keyType}
              </option>
            ))}
          </select>
        </label>
      </div>
    </Modal>
  );
}
