export function formatDate(iso: string) {
	return new Date(iso).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" })
}

const statusStyles: Record<string, string> = {
	Submitted: "bg-slate-200 text-slate-700",
	Acknowledged: "bg-sky-100 text-sky-800",
	InProgress: "bg-amber-100 text-amber-800",
	Resolved: "bg-green-100 text-green-800",
	Closed: "bg-slate-100 text-slate-500 border border-slate-300",
}

const priorityStyles: Record<string, string> = {
	Low: "bg-white text-slate-600 border border-slate-400",
	Medium: "bg-white text-sky-700 border border-sky-700",
	High: "bg-white text-orange-700 border border-orange-700",
	Urgent: "bg-red-700 text-white",
}

export function StatusBadge({ status }: { status: string }) {
	return (
		<span className= {`rounded-full px-3 py-1 text-xs font-medium whitespace-nowrap ${statusStyles[status] ?? "bg-slate-200 text-slate-700"}`
}>
	{ status }
	</span>
	)
}

export function PriorityBadge({ priority }: { priority: string }) {
	return (
		<span className= {`rounded-full px-3 py-1 text-xs font-medium whitespace-nowrap ${priorityStyles[priority] ?? "bg-white text-slate-600 border border-slate-400"}`
}>
	{ priority }
	</span>
	)
}
