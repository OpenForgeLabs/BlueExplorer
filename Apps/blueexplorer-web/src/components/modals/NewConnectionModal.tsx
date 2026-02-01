"use client";

import { useMemo, useState } from "react";
import { Button } from "@/components/Button";
import { Modal } from "@/components/Modal";

type ResourceType = "service-bus" | "redis";

type NewConnectionModalProps = {
  open: boolean;
  onClose: () => void;
};

const DEFAULT_PORT = 6379;

export function NewConnectionModal({
  open,
  onClose,
}: NewConnectionModalProps) {
  const [resourceType, setResourceType] = useState<ResourceType>("service-bus");
  const [displayName, setDisplayName] = useState("");

  const [sbUseManagedIdentity, setSbUseManagedIdentity] = useState(false);
  const [sbConnectionString, setSbConnectionString] = useState("");
  const [sbVaultUri, setSbVaultUri] = useState("");
  const [sbSecretName, setSbSecretName] = useState("");

  const [redisUseConnectionString, setRedisUseConnectionString] =
    useState(true);
  const [redisConnectionString, setRedisConnectionString] = useState("");
  const [redisHost, setRedisHost] = useState("localhost");
  const [redisPort, setRedisPort] = useState(DEFAULT_PORT);
  const [redisPassword, setRedisPassword] = useState("");
  const [redisUseTls, setRedisUseTls] = useState(false);
  const [redisDatabase, setRedisDatabase] = useState<number | "">("");

  const description = useMemo(() => {
    if (resourceType === "redis") {
      return "Add a Redis connection using a connection string or host credentials.";
    }
    return "Add a Service Bus connection using access keys, managed identity, or Key Vault.";
  }, [resourceType]);

  return (
    <Modal
      open={open}
      title="Add New Resource Connection"
      description={description}
      footer={
        <div className="flex flex-wrap items-center justify-between gap-4">
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <div className="flex gap-3">
            <Button variant="ghost" className="gap-2 border border-primary/30">
              <span className="material-symbols-outlined text-lg">bolt</span>
              Test Connection
            </Button>
            <Button>Add Connection</Button>
          </div>
        </div>
      }
    >
      <div className="flex flex-col gap-5">
        <div className="flex flex-col gap-2">
          <label className="text-sm font-medium text-slate-200">
            Resource Type
          </label>
          <div className="relative">
            <select
              className="h-12 w-full appearance-none rounded-lg border border-border-dark bg-surface-dark px-4 text-sm text-slate-200 focus:ring-2 focus:ring-primary/50"
              value={resourceType}
              onChange={(event) =>
                setResourceType(event.target.value as ResourceType)
              }
            >
              <option value="service-bus">Azure Service Bus</option>
              <option value="redis">Redis Cache</option>
            </select>
            <span className="material-symbols-outlined pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-500">
              expand_more
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <label className="text-sm font-medium text-slate-200">
            Display Name
          </label>
          <input
            className="h-12 w-full rounded-lg border border-border-dark bg-surface-dark px-4 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
            placeholder="e.g. Production Redis Cache"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
          />
        </div>

        {resourceType === "service-bus" && (
          <div className="flex flex-col gap-4 rounded-lg border border-border-dark/60 bg-surface-dark/30 p-4">
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-slate-200">
                Connection String
              </label>
              <textarea
                className="min-h-[120px] w-full resize-none rounded-lg border border-border-dark bg-surface-dark p-4 font-mono text-xs text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                placeholder="Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=..."
                value={sbConnectionString}
                onChange={(event) => setSbConnectionString(event.target.value)}
              />
            </div>

            <label className="flex items-center gap-2 text-sm text-slate-200">
              <input
                type="checkbox"
                className="size-4 rounded border-border-dark bg-surface-dark text-primary"
                checked={sbUseManagedIdentity}
                onChange={(event) =>
                  setSbUseManagedIdentity(event.target.checked)
                }
              />
              Use Managed Identity
            </label>

            <div className="grid gap-3 md:grid-cols-2">
              <div className="flex flex-col gap-2">
                <label className="text-sm font-medium text-slate-200">
                  Key Vault URI
                </label>
                <input
                  className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                  placeholder="https://my-vault.vault.azure.net/"
                  value={sbVaultUri}
                  onChange={(event) => setSbVaultUri(event.target.value)}
                />
              </div>
              <div className="flex flex-col gap-2">
                <label className="text-sm font-medium text-slate-200">
                  Key Vault Secret Name
                </label>
                <input
                  className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                  placeholder="servicebus-connection"
                  value={sbSecretName}
                  onChange={(event) => setSbSecretName(event.target.value)}
                />
              </div>
            </div>
          </div>
        )}

        {resourceType === "redis" && (
          <div className="flex flex-col gap-4 rounded-lg border border-border-dark/60 bg-surface-dark/30 p-4">
            <div className="flex flex-wrap gap-3">
              <button
                className={`rounded-full px-3 py-1 text-xs font-semibold ${redisUseConnectionString ? "bg-primary/20 text-primary" : "bg-surface-dark text-slate-400"}`}
                type="button"
                onClick={() => setRedisUseConnectionString(true)}
              >
                Connection String
              </button>
              <button
                className={`rounded-full px-3 py-1 text-xs font-semibold ${!redisUseConnectionString ? "bg-primary/20 text-primary" : "bg-surface-dark text-slate-400"}`}
                type="button"
                onClick={() => setRedisUseConnectionString(false)}
              >
                Host Credentials
              </button>
            </div>

            {redisUseConnectionString ? (
              <div className="flex flex-col gap-2">
                <label className="text-sm font-medium text-slate-200">
                  Connection String
                </label>
                <textarea
                  className="min-h-[120px] w-full resize-none rounded-lg border border-border-dark bg-surface-dark p-4 font-mono text-xs text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                  placeholder="localhost:6379,password=...,ssl=False,defaultDatabase=0"
                  value={redisConnectionString}
                  onChange={(event) =>
                    setRedisConnectionString(event.target.value)
                  }
                />
              </div>
            ) : (
              <div className="grid gap-3 md:grid-cols-2">
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-medium text-slate-200">
                    Host
                  </label>
                  <input
                    className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                    placeholder="localhost"
                    value={redisHost}
                    onChange={(event) => setRedisHost(event.target.value)}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-medium text-slate-200">
                    Port
                  </label>
                  <input
                    className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                    type="number"
                    value={redisPort}
                    onChange={(event) =>
                      setRedisPort(Number(event.target.value))
                    }
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-medium text-slate-200">
                    Password
                  </label>
                  <input
                    className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                    placeholder="optional"
                    value={redisPassword}
                    onChange={(event) => setRedisPassword(event.target.value)}
                  />
                </div>
                <div className="flex flex-col gap-2">
                  <label className="text-sm font-medium text-slate-200">
                    Database
                  </label>
                  <input
                    className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50"
                    type="number"
                    placeholder="0"
                    value={redisDatabase}
                    onChange={(event) =>
                      setRedisDatabase(
                        event.target.value === ""
                          ? ""
                          : Number(event.target.value),
                      )
                    }
                  />
                </div>
                <label className="flex items-center gap-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    className="size-4 rounded border-border-dark bg-surface-dark text-primary"
                    checked={redisUseTls}
                    onChange={(event) => setRedisUseTls(event.target.checked)}
                  />
                  Use TLS (SSL)
                </label>
              </div>
            )}
          </div>
        )}

        <div className="flex gap-3 rounded-lg border border-primary/20 bg-primary/10 p-4">
          <span className="material-symbols-outlined text-primary text-xl">
            info
          </span>
          <p className="text-sm text-slate-300">
            BlueExplorer stores credentials securely. For Azure resources, use
            Shared Access Policies or Key Vault. For Redis, supply either a
            connection string or host credentials with optional TLS.
          </p>
        </div>
      </div>
    </Modal>
  );
}
