import { type InputHTMLAttributes } from "react";

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  hasIcon?: boolean;
};

export function Input({ className = "", hasIcon, ...props }: InputProps) {
  return (
    <input
      className={`w-full rounded-lg border border-border-dark bg-surface-dark py-2 text-sm text-slate-200 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/50 ${hasIcon ? "pl-10 pr-4" : "px-4"} ${className}`}
      {...props}
    />
  );
}
