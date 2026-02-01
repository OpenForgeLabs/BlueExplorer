import { type HTMLAttributes } from "react";

export type CardProps = HTMLAttributes<HTMLDivElement>;

export function Card({ className = "", ...props }: CardProps) {
  return (
    <div
      className={`rounded-xl border border-border-dark bg-surface-dark p-6 transition-all hover:ring-2 hover:ring-primary/40 ${className}`}
      {...props}
    />
  );
}
