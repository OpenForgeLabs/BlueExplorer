import type { Metadata } from "next";
import { Suspense } from "react";
import "../styles/globals.css";
import { AsyncProvider } from "@/lib/async/AsyncContext";
import { GlobalLoadingOverlay } from "@/components/feedback/GlobalLoadingOverlay";
import { AppShell } from "@/components/layout/AppShell";

export const metadata: Metadata = {
  title: "BlueExplorer",
  description: "Azure-first infrastructure explorer",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        {/* eslint-disable-next-line @next/next/no-page-custom-font */}
        <link
          rel="stylesheet"
          href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght@100..700&display=swap"
        />
      </head>
      <body className="antialiased bg-background text-slate-100">
        <AsyncProvider>
          <Suspense fallback={null}>
            <AppShell>{children}</AppShell>
          </Suspense>
          <GlobalLoadingOverlay />
        </AsyncProvider>
      </body>
    </html>
  );
}
