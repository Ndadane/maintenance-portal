import { useEffect, useState } from "react"
import { Link } from "react-router-dom"
import { landlordApi } from "../api/landlord"
import { friendlyError } from "../api/errorMessage"
import type { PropertyResponse, UnitResponse, MaintenanceRequestResponse } from "../api/types"
import { formatDate, StatusBadge, PriorityBadge } from "../api/format"

export function LandlordDashboard() {
	const [properties, setProperties] = useState<PropertyResponse[]>([])
	const [selectedPropertyId, setSelectedPropertyId] = useState<string | null>(null)
	const [units, setUnits] = useState<UnitResponse[]>([])
	const [requests, setRequests] = useState<MaintenanceRequestResponse[]>([])
	const [error, setError] = useState<string | null>(null)
	const [loading, setLoading] = useState(true)

	const [newName, setNewName] = useState("")
	const [newAddress, setNewAddress] = useState("")
	const [newUnitLabel, setNewUnitLabel] = useState("")
	const [tenantEmail, setTenantEmail] = useState("")
	const [moveInDate, setMoveInDate] = useState("")
	const [assignUnitId, setAssignUnitId] = useState<string | null>(null)
	const [assignResult, setAssignResult] = useState<string | null>(null)

	async function loadProperties() {
		try {
			setProperties(await landlordApi.getProperties())
		} catch (err) {
			setError(friendlyError(err, "Could not load properties."))
		}
	}

	async function loadRequests(propertyId?: string) {
		try {
			setRequests(await landlordApi.getRequests(propertyId))
		} catch (err) {
			setError(friendlyError(err, "Could not load requests."))
		}
	}

	useEffect(() => {
		(async () => {
			setLoading(true)
			await Promise.all([loadProperties(), loadRequests()])
			setLoading(false)
		})()
	}, [])

	async function selectProperty(id: string) {
		setSelectedPropertyId(id)
		setAssignUnitId(null)
		setAssignResult(null)
		try {
			setUnits(await landlordApi.getUnits(id))
		} catch (err) {
			setError(friendlyError(err, "Could not load units."))
		}
		await loadRequests(id)
	}

	async function handleCreateProperty(e: React.FormEvent) {
		e.preventDefault()
		setError(null)
		try {
			await landlordApi.createProperty(newName, newAddress)
			setNewName("")
			setNewAddress("")
			await loadProperties()
		} catch (err) {
			setError(friendlyError(err, "Could not create property."))
		}
	}

	async function handleCreateUnit(e: React.FormEvent) {
		e.preventDefault()
		if (!selectedPropertyId) return
		setError(null)
		try {
			await landlordApi.createUnit(selectedPropertyId, newUnitLabel)
			setNewUnitLabel("")
			setUnits(await landlordApi.getUnits(selectedPropertyId))
		} catch (err) {
			setError(friendlyError(err, "Could not create unit."))
		}
	}

	async function handleAssignTenant(e: React.FormEvent) {
		e.preventDefault()
		if (!assignUnitId) return
		setError(null)
		setAssignResult(null)
		try {
			const res = await landlordApi.assignTenant(assignUnitId, tenantEmail, moveInDate)
			setAssignResult(
				res.temporaryPassword
					? `Tenant account created. Temporary password: ${res.temporaryPassword} (relay this to the tenant)`
					: "Tenant assigned."
			)
			setTenantEmail("")
			setMoveInDate("")
		} catch (err) {
			setError(friendlyError(err, "Could not assign tenant."))
		}
	}

	async function handleStatusChange(requestId: string, status: string) {
		if (status === "Closed" && !window.confirm("Close this request? This usually can't be undone.")) return
		setError(null)
		try {
			await landlordApi.updateStatus(requestId, status)
			await loadRequests(selectedPropertyId ?? undefined)
		} catch (err) {
			setError(friendlyError(err, "Could not update status."))
		}
	}

	if (loading) {
		return <p className="p-6 text-sm text-muted">Loading your properties...</p>
	}

	return (
		<div className="mx-auto max-w-4xl p-4">
			{error && <p className="mb-3 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

			<section className="mb-6 rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">Properties</h2>
				<div className="mb-3 flex flex-wrap gap-2">
					{properties.map((p) => (
						<button
							key={p.id}
							onClick={() => selectProperty(p.id)}
							className={`rounded-md border px-3 py-2 text-sm ${selectedPropertyId === p.id
									? "border-brand bg-brand text-white"
									: "border-line text-ink hover:border-brand"
								}`}
						>
							{p.name}
						</button>
					))}
					{properties.length === 0 && (
						<p className="text-sm text-muted">No properties yet. Add your first one below to get started.</p>
					)}
				</div>
				<form onSubmit={handleCreateProperty} className="flex flex-col gap-2 sm:flex-row sm:flex-wrap">
					<input
						placeholder="Property name"
						required
						value={newName}
						onChange={(e) => setNewName(e.target.value)}
						className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
					/>
					<input
						placeholder="Address"
						required
						value={newAddress}
						onChange={(e) => setNewAddress(e.target.value)}
						className="min-w-0 flex-1 rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
					/>
					<button type="submit" className="rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark">
						Add property
					</button>
				</form>
			</section>

			{selectedPropertyId && (
				<section className="mb-6 rounded-lg border border-line bg-white p-4">
					<h2 className="mb-3 text-sm font-medium text-muted">Units</h2>
					<ul className="mb-3 flex flex-col gap-2">
						{units.map((u) => (
							<li key={u.id} className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-line px-3 py-2 text-sm">
								<span>{u.unitLabel}</span>
								<button
									onClick={() => { setAssignUnitId(u.id); setAssignResult(null) }}
									className="text-brand hover:underline"
								>
									Assign tenant
								</button>
							</li>
						))}
						{units.length === 0 && <p className="text-sm text-muted">No units yet. Add one below.</p>}
					</ul>
					<form onSubmit={handleCreateUnit} className="mb-4 flex flex-col gap-2 sm:flex-row">
						<input
							placeholder="Unit label (e.g. 2B)"
							required
							value={newUnitLabel}
							onChange={(e) => setNewUnitLabel(e.target.value)}
							className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
						/>
						<button type="submit" className="rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark">
							Add unit
						</button>
					</form>

					{assignUnitId && (
						<form onSubmit={handleAssignTenant} className="flex flex-col gap-2 border-t border-line pt-3 sm:flex-row sm:flex-wrap sm:items-end">
							<label className="flex flex-col gap-1 text-sm text-ink">
								Tenant email
								<input
									type="email"
									required
									value={tenantEmail}
									onChange={(e) => setTenantEmail(e.target.value)}
									className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
								/>
							</label>
							<label className="flex flex-col gap-1 text-sm text-ink">
								Move-in date
								<input
									type="date"
									required
									value={moveInDate}
									onChange={(e) => setMoveInDate(e.target.value)}
									className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
								/>
							</label>
							<button type="submit" className="rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark">
								Assign
							</button>
						</form>
					)}
					{assignResult && <p className="mt-2 text-sm text-green-700">{assignResult}</p>}
				</section>
			)}

			<section className="rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">
					Requests {selectedPropertyId ? "(this property)" : "(all properties)"}
				</h2>
				<ul className="flex flex-col gap-2">
					{requests.map((r) => (
						<li key={r.id} className="rounded-md border border-line p-3">
							<div className="flex flex-wrap items-center justify-between gap-2">
								<Link to={`/requests/${r.id}`} className="font-medium text-ink hover:text-brand">
									{r.title}
								</Link>
								<div className="flex gap-2">
									<PriorityBadge priority={r.priority} />
									<StatusBadge status={r.status} />
								</div>
							</div>
							<p className="mt-1 text-xs text-muted">
								{r.propertyName} · {r.unitLabel} · {formatDate(r.createdAt)}
							</p>
							<div className="mt-2 flex flex-wrap gap-1">
								{["Acknowledged", "InProgress", "Resolved", "Closed"].map((s) => (
									<button
										key={s}
										onClick={() => handleStatusChange(r.id, s)}
										disabled={r.status === s}
										className="rounded-md border border-line px-2 py-1 text-xs text-ink hover:border-brand disabled:opacity-40"
									>
										{s}
									</button>
								))}
							</div>
						</li>
					))}
					{requests.length === 0 && (
						<p className="text-sm text-muted">
							No requests {selectedPropertyId ? "for this property" : ""} yet. They'll appear here once a tenant submits one.
						</p>
					)}
				</ul>
			</section>
		</div>
	)
}