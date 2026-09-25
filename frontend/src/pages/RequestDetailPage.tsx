import { useEffect, useState, useCallback } from "react"
import { useParams, Link } from "react-router-dom"
import { useAuth } from "../auth/AuthContext"
import { requestDetailApi } from "../api/requestDetail"
import { ApiError } from "../api/client"
import type { MaintenanceRequestResponse, CommentResponse, PhotoResponse } from "../api/types"
import { formatDate, StatusBadge, PriorityBadge } from "../api/format"

const STATUSES = ["Submitted", "Acknowledged", "InProgress", "Resolved", "Closed"]
const PRIORITIES = ["Low", "Medium", "High", "Urgent"]
const MAX_PHOTO_BYTES = 10 * 1024 * 1024

export function RequestDetailPage() {
	const { id } = useParams<{ id: string }>()
	const { isLandlord } = useAuth()

	const [request, setRequest] = useState<MaintenanceRequestResponse | null>(null)
	const [comments, setComments] = useState<CommentResponse[]>([])
	const [photos, setPhotos] = useState<PhotoResponse[]>([])
	const [error, setError] = useState<string | null>(null)
	const [notFound, setNotFound] = useState(false)

	const [commentBody, setCommentBody] = useState("")
	const [posting, setPosting] = useState(false)
	const [uploading, setUploading] = useState(false)

	const load = useCallback(async () => {
		if (!id) return
		try {
			const [req, cmts, phs] = await Promise.all([
				requestDetailApi.get(id),
				requestDetailApi.getComments(id),
				requestDetailApi.getPhotos(id),
			])
			setRequest(req)
			setComments(cmts)
			setPhotos(phs)
		} catch (err) {
			if (err instanceof ApiError && err.status === 404) setNotFound(true)
			else setError(err instanceof ApiError ? err.message : "Could not load this request.")
		}
	}, [id])

	useEffect(() => { load() }, [load])

	async function handleStatusChange(status?: string, priority?: string) {
		if (!id) return
		setError(null)
		try {
			setRequest(await requestDetailApi.updateStatus(id, status, priority))
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Could not update the request.")
		}
	}

	async function handleAddComment(e: React.FormEvent) {
		e.preventDefault()
		if (!id || !commentBody.trim()) return
		setPosting(true)
		setError(null)
		try {
			const comment = await requestDetailApi.addComment(id, commentBody.trim())
			setComments((prev) => [...prev, comment])
			setCommentBody("")
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Could not post comment.")
		} finally {
			setPosting(false)
		}
	}

	async function handlePhotoSelected(e: React.ChangeEvent<HTMLInputElement>) {
		const file = e.target.files?.[0]
		e.target.value = ""
		if (!file || !id) return

		if (!["image/jpeg", "image/png", "image/webp"].includes(file.type)) {
			setError("Only JPEG, PNG and WEBP images are allowed.")
			return
		}
		if (file.size > MAX_PHOTO_BYTES) {
			setError("Photo must be at most 10 MB.")
			return
		}

		setUploading(true)
		setError(null)
		try {
			const presign = await requestDetailApi.presignPhoto(id, file.type, file.size)
			const uploadRes = await fetch(presign.uploadUrl, {
				method: "PUT",
				headers: { "Content-Type": presign.contentType },
				body: file,
			})
			if (!uploadRes.ok) throw new Error("Upload to storage failed.")

			const photo = await requestDetailApi.confirmPhoto(id, presign.key)
			setPhotos((prev) => [...prev, photo])
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Could not upload photo.")
		} finally {
			setUploading(false)
		}
	}

	if (notFound) {
		return (
			<div className="mx-auto max-w-2xl p-6">
				<p className="text-ink">This request doesn't exist or you don't have access to it.</p>
				<Link to={isLandlord ? "/landlord" : "/tenant"} className="text-brand hover:underline">
					Back
				</Link>
			</div>
		)
	}

	if (!request) {
		return <div className="mx-auto max-w-2xl p-6 text-muted">{error ?? "Loading..."}</div>
	}

	return (
		<div className="mx-auto max-w-2xl p-4">
			<Link to={isLandlord ? "/landlord" : "/tenant"} className="mb-3 inline-block text-sm text-brand hover:underline">
				← Back
			</Link>

			{error && <p className="mb-3 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p>}

			<div className="rounded-lg border border-line bg-white p-4">
				<div className="flex items-center justify-between gap-2">
					<h1 className="text-lg font-medium text-ink">{request.title}</h1>
					<div className="flex gap-2">
						<PriorityBadge priority={request.priority} />
						<StatusBadge status={request.status} />
					</div>
				</div>
				<p className="mt-1 text-sm text-muted">
					{request.propertyName} · {request.unitLabel} · {request.category} · {formatDate(request.createdAt)}
				</p>
				<p className="mt-3 text-sm text-ink">{request.description}</p>

				{isLandlord && (
					<div className="mt-4 flex flex-wrap gap-4 border-t border-line pt-3">
						<div>
							<p className="mb-1 text-xs text-muted">Status</p>
							<div className="flex flex-wrap gap-1">
								{STATUSES.map((s) => (
									<button
										key={s}
										onClick={() => handleStatusChange(s, undefined)}
										disabled={request.status === s}
										className="rounded-md border border-line px-2 py-1 text-xs text-ink hover:border-brand disabled:opacity-40"
									>
										{s}
									</button>
								))}
							</div>
						</div>
						<div>
							<p className="mb-1 text-xs text-muted">Priority</p>
							<div className="flex flex-wrap gap-1">
								{PRIORITIES.map((p) => (
									<button
										key={p}
										onClick={() => handleStatusChange(undefined, p)}
										disabled={request.priority === p}
										className="rounded-md border border-line px-2 py-1 text-xs text-ink hover:border-brand disabled:opacity-40"
									>
										{p}
									</button>
								))}
							</div>
						</div>
					</div>
				)}
			</div>

			<div className="mt-4 rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">Photos</h2>
				<div className="mb-3 flex flex-wrap gap-2">
					{photos.map((p) => (
						<a key={p.id} href={p.downloadUrl} target="_blank" rel="noreferrer">
							<img
								src={p.downloadUrl}
								alt="Request"
								className="h-24 w-24 rounded-md border border-line object-cover"
							/>
						</a>
					))}
					{photos.length === 0 && <p className="text-sm text-muted">No photos yet.</p>}
				</div>
				<label className="inline-block cursor-pointer rounded-md border border-line px-3 py-2 text-sm text-ink hover:border-brand">
					{uploading ? "Uploading..." : "Add photo"}
					<input
						type="file"
						accept="image/jpeg,image/png,image/webp"
						onChange={handlePhotoSelected}
						disabled={uploading}
						className="hidden"
					/>
				</label>
			</div>

			<div className="mt-4 rounded-lg border border-line bg-white p-4">
				<h2 className="mb-3 text-sm font-medium text-muted">Comments</h2>
				<ul className="mb-3 flex flex-col gap-2">
					{comments.map((c) => (
						<li key={c.id} className="rounded-md bg-page p-2">
							<div className="flex items-center justify-between text-xs text-muted">
								<span className="font-medium text-ink">
									{c.authorEmail} <span className="font-normal text-muted">({c.authorRole})</span>
								</span>
								<span>{formatDate(c.createdAt)}</span>
							</div>
							<p className="mt-1 text-sm text-ink">{c.body}</p>
						</li>
					))}
					{comments.length === 0 && <p className="text-sm text-muted">No comments yet.</p>}
				</ul>
				<form onSubmit={handleAddComment} className="flex gap-2">
					<input
						placeholder="Write a comment..."
						value={commentBody}
						onChange={(e) => setCommentBody(e.target.value)}
						maxLength={2000}
						className="flex-1 rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
					/>
					<button
						type="submit"
						disabled={posting || !commentBody.trim()}
						className="rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark disabled:opacity-60"
					>
						Post
					</button>
				</form>
			</div>
		</div>
	)
}
