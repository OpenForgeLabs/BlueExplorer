import { type ButtonHTMLAttributes } from "react";

type Variant = "primary" | "secondary" | "ghost";
type Size = "sm" | "md";

const variants: Record<Variant, string> = {
  primary:
    "bg-primary text-white shadow-lg shadow-primary/20 hover:bg-primary/90",
  secondary:
    "bg-surface-dark text-slate-200 border border-border-dark hover:bg-slate-700/60",
  ghost: "text-slate-400 hover:bg-surface-dark",
};

const sizes: Record<Size, string> = {
  sm: "px-3 py-1.5 text-xs font-bold",
  md: "px-4 py-2.5 text-sm font-semibold",
};

export type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: Variant;
  size?: Size;
};

export function Button({
  className = "",
  variant = "primary",
  size = "md",
  ...props
}: ButtonProps) {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-lg transition-colors ${variants[variant]} ${sizes[size]} ${className}`}
      {...props}
    />
  );
}
