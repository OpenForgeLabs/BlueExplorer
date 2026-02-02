"use client";

import { useMemo, useState } from "react";
import { Button } from "@/components/Button";
import { Modal } from "@/components/Modal";
import { RedisConnectionUpsertRequest } from "@/lib/types";

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
  const [redisEnvironment, setRedisEnvironment] = useState<
    "production" | "staging" | "development"
  >("development");
  const [testStatus, setTestStatus] = useState<
    "idle" | "loading" | "success" | "error"
  >("idle");
  const [testMessage, setTestMessage] = useState("");
  const [saveStatus, setSaveStatus] = useState<
    "idle" | "loading" | "success" | "error"
  >("idle");
  const [saveMessage, setSaveMessage] = useState("");

  const description = useMemo(() => {
    if (resourceType === "redis") {
      return "Add a Redis connection using a connection string or host credentials.";
    }
    return "Add a Service Bus connection using access keys, managed identity, or Key Vault.";
  }, [resourceType]);

  const buildRedisRequest = (): RedisConnectionUpsertRequest => ({
    name: displayName.trim(),
    connectionString: redisUseConnectionString
      ? redisConnectionString.trim() || null
      : null,
    host: redisUseConnectionString ? "" : redisHost.trim(),
    port: redisPort,
    password: redisUseConnectionString ? null : redisPassword.trim() || null,
    useTls: redisUseTls,
    database: redisDatabase === "" ? null : redisDatabase,
    environment: redisEnvironment,
  });

  const handleTestConnection = async () => {
    setTestStatus("loading");
    setTestMessage("");
    setSaveStatus("idle");
    setSaveMessage("");

    if (resourceType !== "redis") {
      setTestStatus("error");
      setTestMessage("Test connection is only available for Redis right now.");
      return;
    }

    const requestBody = buildRedisRequest();
    if (!requestBody.name) {
      setTestStatus("error");
      setTestMessage("Display name is required to test the connection.");
      return;
    }

    const response = await fetch("/api/redis/connections/test", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
      setTestStatus("error");
      setTestMessage("Failed to reach the Redis test endpoint.");
      return;
    }

    const data = (await response.json()) as {
      isSuccess: boolean;
      message?: string;
      reasons?: string[];
    };

    if (!data.isSuccess) {
      setTestStatus("error");
      setTestMessage(
        data.reasons?.[0] ?? data.message ?? "Test connection failed.",
      );
      return;
    }

    setTestStatus("success");
    setTestMessage("Connection successful.");
  };

  const handleAddConnection = async () => {
    setSaveStatus("loading");
    setSaveMessage("");
    setTestStatus("idle");
    setTestMessage("");

    if (!displayName.trim()) {
      setSaveStatus("error");
      setSaveMessage("Display name is required.");
      return;
    }

    if (resourceType !== "redis") {
      setSaveStatus("error");
      setSaveMessage("Service Bus creation is not wired yet.");
      return;
    }

    const requestBody = buildRedisRequest();
    const response = await fetch("/api/redis/connections", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
      setSaveStatus("error");
      setSaveMessage("Failed to reach the Redis connections endpoint.");
      return;
    }

    const data = (await response.json()) as {
      isSuccess: boolean;
      message?: string;
      reasons?: string[];
    };

    if (!data.isSuccess) {
      setSaveStatus("error");
      setSaveMessage(
        data.reasons?.[0] ?? data.message ?? "Failed to save connection.",
      );
      return;
    }

    setSaveStatus("success");
    setSaveMessage("Connection saved.");
    window.dispatchEvent(new Event("resources:refresh"));
    onClose();
  };

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
            <Button
              variant="ghost"
              className="gap-2 border border-primary/30"
              onClick={handleTestConnection}
              disabled={testStatus === "loading"}
            >
              <span className="material-symbols-outlined text-lg">bolt</span>
              {testStatus === "loading" ? "Testing..." : "Test Connection"}
            </Button>
            <Button
              onClick={handleAddConnection}
              disabled={saveStatus === "loading"}
            >
              {saveStatus === "loading" ? "Saving..." : "Add Connection"}
            </Button>
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

            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-slate-200">
                Environment
              </label>
              <select
                className="h-11 w-full rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200"
                value={redisEnvironment}
                onChange={(event) =>
                  setRedisEnvironment(
                    event.target.value as "production" | "staging" | "development",
                  )
                }
              >
                <option value="development">development</option>
                <option value="staging">staging</option>
                <option value="production">production</option>
              </select>
            </div>
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
        {testStatus !== "idle" && (
          <div
            className={`rounded-lg border px-4 py-3 text-sm ${
              testStatus === "success"
                ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-200"
                : testStatus === "error"
                  ? "border-rose-500/40 bg-rose-500/10 text-rose-200"
                  : "border-border-dark bg-surface-dark/60 text-slate-300"
            }`}
          >
            {testStatus === "loading" ? "Testing connection..." : testMessage}
          </div>
        )}
        {saveStatus !== "idle" && (
          <div
            className={`rounded-lg border px-4 py-3 text-sm ${
              saveStatus === "success"
                ? "border-emerald-500/40 bg-emerald-500/10 text-emerald-200"
                : saveStatus === "error"
                  ? "border-rose-500/40 bg-rose-500/10 text-rose-200"
                  : "border-border-dark bg-surface-dark/60 text-slate-300"
            }`}
          >
            {saveStatus === "loading" ? "Saving connection..." : saveMessage}
          </div>
        )}
      </div>
    </Modal>
  );
}
