export type ApiResponse<T> = {
  isSuccess: boolean;
  message: string;
  reasons: string[];
  data: T;
};

export type ResourceType = "all" | "service-bus" | "redis";

export type ResourceStatus = "connected" | "warning" | "offline";

export type ResourceSummaryBase = {
  id: string;
  name: string;
  type: "service-bus" | "redis";
  environment: "production" | "staging" | "development";
  endpoint: string;
  status?: ResourceStatus;
};

export type ServiceBusResourceSummary = ResourceSummaryBase & {
  type: "service-bus";
  messageRate?: string;
  activeQueues?: number;
};

export type RedisResourceSummary = ResourceSummaryBase & {
  type: "redis";
  opsRate?: string;
  memoryUsage?: string;
  keyCount?: number;
};

export type ResourceSummary = ServiceBusResourceSummary | RedisResourceSummary;

export type ResourceCatalog = {
  serviceBus: ServiceBusResourceSummary[];
  redis: RedisResourceSummary[];
};

export type KeyVaultSecretConfig = {
  vaultUri: string;
  secretName: string;
};

export type ServiceBusConnectionUpsertRequest = {
  name: string;
  useManagedIdentity: boolean;
  connectionString?: string;
  keyVault?: KeyVaultSecretConfig | null;
};

export type RedisConnectionUpsertRequest = {
  name: string;
  connectionString?: string | null;
  host: string;
  port: number;
  password?: string | null;
  useTls: boolean;
  database?: number | null;
  environment?: "production" | "staging" | "development";
};

export type RedisConnectionInfo = {
  name: string;
  useTls: boolean;
  database?: number | null;
  isEditable: boolean;
  source: string;
  environment?: "production" | "staging" | "development";
};

export type RedisKeyScanResult = {
  keys: string[];
  cursor: number;
};

export type RedisKeyScanResultWithInfo = {
  keys: RedisKeyInfo[];
  cursor: number;
};

export type RedisKeyType =
  | "string"
  | "hash"
  | "list"
  | "set"
  | "zset"
  | "stream"
  | "unknown";

export type RedisKeyInfo = {
  key: string;
  type: RedisKeyType;
  ttlSeconds?: number | null;
};

export type RedisZSetEntry = {
  member: string;
  score: number;
};

export type RedisStreamEntry = {
  id: string;
  values: Record<string, string>;
};

export type RedisKeyValue =
  | { type: "string"; value: string | null }
  | { type: "hash"; value: Record<string, string> }
  | { type: "list"; value: string[] }
  | { type: "set"; value: string[] }
  | { type: "zset"; value: RedisZSetEntry[] }
  | { type: "stream"; value: RedisStreamEntry[] }
  | { type: "unknown"; value: unknown };

export type RedisServerInfo = {
  sections: Record<string, Record<string, string>>;
};

export type RedisServerStats = {
  version?: string;
  uptimeSeconds?: number;
  connectedClients?: number;
  opsPerSec?: number;
  usedMemoryHuman?: string;
  keyspace?: string;
};
