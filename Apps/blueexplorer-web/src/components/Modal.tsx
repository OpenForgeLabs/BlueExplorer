import { type ReactNode } from "react";

type ModalProps = {
  open: boolean;
  title: string;
  description?: string;
  children: ReactNode;
  footer?: ReactNode;
};

export function Modal({
  open,
  title,
  description,
  children,
  footer,
}: ModalProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-6">
      <div className="w-full max-w-xl rounded-xl border border-border-dark bg-background shadow-2xl">
        <div className="border-b border-border-dark px-6 py-4">
          <h3 className="text-lg font-semibold text-slate-100">{title}</h3>
          {description ? (
            <p className="mt-1 text-sm text-slate-400">{description}</p>
          ) : null}
        </div>
        <div className="px-6 py-5 text-slate-100">{children}</div>
        {footer ? (
          <div className="border-t border-border-dark bg-background px-6 py-4">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  );
}
