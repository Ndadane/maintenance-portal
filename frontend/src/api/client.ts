const BASE_URL = "http://localhost:5080/api"

export class ApiError extends Error {
	status: number
	constructor(status: number, message: string) {
		super(message)
		this.status = status
	}
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
	const token = localStorage.getItem("token")
	const headers: Record<string, string> = {
		"Content-Type": "application/json",
		...(options.headers as Record<string, string>),
	}
	if (token) headers["Authorization"] = `Bearer ${token}`

	const res = await fetch(`${BASE_URL}${path}`, { ...options, headers })

	if (!res.ok) {
		let message = res.statusText
		try {
			const body = await res.json()
			message = body.title ?? body.detail ?? JSON.stringify(body.errors ?? body)
		} catch {
			// response had no JSON body
		}
		throw new ApiError(res.status, message)
	}

	if (res.status === 204) return undefined as T
	return res.json() as Promise<T>
}

export const api = {
	get: <T>(path: string) => request<T>(path),
	post: <T>(path: string, body?: unknown) =>
		request<T>(path, { method: "POST", body: body ? JSON.stringify(body) : undefined }),
	patch: <T>(path: string, body?: unknown) =>
		request<T>(path, { method: "PATCH", body: body ? JSON.stringify(body) : undefined }),
}
