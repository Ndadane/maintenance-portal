import { useEffect, useState } from "react"
import { Link } from "react-router-dom"
import { tenantApi, type MyUnitResponse } from "../api/tenant"
import { ApiError } from "../api/client"
import type { MaintenanceRequestResponse } from "../api/types"
import { formatDate, StatusBadge, PriorityBadge } from "../api/format"

const CATEGORIES = ["Plumbing", "Electrical", "HVAC", "Appliance", "Structural", "Pest", "Other"]
const PRIORITIES = ["Low", "Medium", "High", "Urgent"]

export function TenantView() {
	const [unit, setUnit] = useState<MyUnitResponse | null>(null)
	const [requests, setRequests] = useState<MaintenanceRequestResponse[]>([])
	const [error, setError] = useState<string | null>(null)

	const [title, setTitle] = useState("")
	const [description, setDescription] = useState("")
	const [category, setCategory] = useState(CATEGORIES[0])
	const [priority, setPriority] = useState(PRIORITIES[0])
	const [submitting, setSubmitting] = useState(false)

	async function loadAll() {
		try {
			const [u, reqs] = await Promise.all([tenantApi.getMyUnit(), tenantApi.getRequests()])
			setUnit(u)
			setRequests(reqs)
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Could not load your unit.")
		}
	}

	useEffect(() => { loadAll() }, [])

	async function handleSubmit(e: React.FormEvent) {
		e.preventDefault()
		setSubmitting(true)
		setError(null)
		try {
			await tenantApi.createRequest(title, description, category, priority)
			setTitle("")
			setDescription("")
			setCategory(CATEGORIES[0])
			setPriority(PRIORITIES[0])
			setRequests(await tenantApi.getRequests())
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Could not submit request.")
		} finally {
			setSubmitting(false)
		}
	}

	return (
		<div className="mx-auto max-w-2xl p-4">
			{error && <p className="mb-3 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

			{unit && (
				<section className="mb-6 rounded-lg border border-line bg-white p-4">
					<h2 className="mb-1 text-sm font-medium text-muted">Your unit</h2>
					<p className="text-ink">{unit.propertyName} · {unit.unitLabel}</p>
					<p className="text-sm text-muted">{unit.propertyAddress}</p>
					<p className="mt-1 text-xs text-muted">Moved in {formatDate(unit.moveInDate)}</p>
				</section>
			)}

			<section className="mb-6 rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">New request</h2>
				<form onSubmit={handleSubmit} className="flex flex-col gap-2">
					<input
						placeholder="Title (e.g. Kitchen sink is leaking)"
						required
						value={title}
						onChange={(e) => setTitle(e.target.value)}
						className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
					/>
					<textarea
						placeholder="Describe the problem"
						required
						rows={3}
						value={description}
						onChange={(e) => setDescription(e.target.value)}
						className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
					/>
					<div className="flex gap-2">
						<select
							value={category}
							onChange={(e) => setCategory(e.target.value)}
							className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
						>
							{CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
						</select>
						<select
							value={priority}
							onChange={(e) => setPriority(e.target.value)}
							className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
						>
							{PRIORITIES.map((p) => <option key={p} value={p}>{p}</option>)}
						</select>
					</div>
					<button
						type="submit"
						disabled={submitting || !unit}
						className="self-start rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark disabled:opacity-60"
					>
						{submitting ? "Submitting..." : "Submit request"}
					</button>
				</form>
			</section>

			<section className="rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">Your requests</h2>
				<ul className="flex flex-col gap-2">
					{requests.map((r) => (
						<li key={r.id} className="rounded-md border border-line p-3">
							<div className="flex items-center justify-between gap-2">
								<Link to={`/requests/${r.id}`} className="font-medium text-ink hover:text-brand">
									{r.title}
								</Link>
								<div className="flex gap-2">
									<PriorityBadge priority={r.priority} />
									<StatusBadge status={r.status} />
								</div>
							</div>
							<p className="mt-1 text-xs text-muted">{r.category} · {formatDate(r.createdAt)}</p>
						</li>
					))}
					{requests.length === 0 && <p className="text-sm text-muted">No requests yet.</p>}
				</ul>
			</section>
		</div>
	)
}
