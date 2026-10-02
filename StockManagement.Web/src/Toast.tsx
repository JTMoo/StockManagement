import { CheckCircle2, Info, XCircle } from "lucide-react";
import { createContext, useContext, useRef, useState, type ReactNode } from "react";

/** Transient save confirmation (Toast spec): bottom-right, auto-dismisses after 4s or on click. */
export type ToastTone = "success" | "danger" | "info";
type ToastItem = { id: number; tone: ToastTone; message: string };

const icons = { success: CheckCircle2, danger: XCircle, info: Info };

const ToastContext = createContext<{ show: (message: string, tone?: ToastTone) => void } | null>(null);

export function ToastProvider({ children }: { children: ReactNode })
{
	const [toasts, setToasts] = useState<ToastItem[]>([]);
	const nextId = useRef(0);

	function dismiss(id: number)
	{
		setToasts(current => current.filter(toast => toast.id !== id));
	}

	function show(message: string, tone: ToastTone = "success")
	{
		const id = nextId.current++;
		setToasts(current => [...current, { id, tone, message }]);
		setTimeout(() => dismiss(id), 4000);
	}

	return (
		<ToastContext.Provider value={{ show }}>
			{children}
			<div className="toast-stack">
				{toasts.map(toast =>
				{
					const Icon = icons[toast.tone];
					return (
						<div key={toast.id} role="status" className="toast" onClick={() => dismiss(toast.id)}>
							<Icon className={`icon ${toast.tone}`} size={18} />
							{toast.message}
						</div>
					);
				})}
			</div>
		</ToastContext.Provider>
	);
}

export function useToast()
{
	const context = useContext(ToastContext);
	if (!context) throw new Error("useToast needs a ToastProvider.");
	return context;
}
