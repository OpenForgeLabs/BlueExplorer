import { redirect } from "next/navigation";

export default async function RedisOverviewPage({
  params,
}: {
  params: Promise<{ connectionName: string }>;
}) {
  const { connectionName } = await params;
  redirect(`/redis/${encodeURIComponent(connectionName)}/keys`);
}
