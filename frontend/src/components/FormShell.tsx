import type { ReactNode, FormEvent } from "react"

export function FormShell({
	title, error, onSubmit, children, footer,
}: {
	title: string
	error: string | null
	onSubmit: (e: FormEvent) => void
	children: ReactNode
	footer?: ReactNode
}) {
	return (
		<div className="mx-auto mt-16 max-w-sm rounded-lg border border-line bg-white p-6 shadow-sm">
			<h1 className="mb-4 text-lg font-medium text-ink">{title}</h1>
			<form onSubmit={onSubmit} className="flex flex-col gap-3">
				{children}
				{error && <p className="text-sm text-red-600">{error}</p>}
			</form>
			{footer && <div className="mt-4 text-sm text-muted">{footer}</div>}
		</div>
	)
}

export function TextField({
	label, ...props
}: { label: string } & React.InputHTMLAttributes<HTMLInputElement>) {
	return (
		<label className="flex flex-col gap-1 text-sm text-ink">
			{label}
			<input
				{...props}
				className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
			/>
		</label>
	)
}
