export default async function ServiceBusConnectionPage({
  params,
}: {
  params: Promise<{ connectionName: string }>;
}) {
  const { connectionName } = await params;
  return (
    <div className="flex-1 overflow-y-auto bg-background/50 px-6 pb-6 lg:px-8 lg:pb-8">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-slate-100">
          {connectionName}
        </h2>
        <p className="text-sm text-slate-400">
          Service Bus details are coming next. This page is a placeholder.
        </p>
      </div>

      <div className="rounded-xl border border-dashed border-border-dark p-8 text-center text-sm text-slate-400">
        Detail views for queues, topics, and subscriptions will live here.
      </div>
    </div>
  );
}
