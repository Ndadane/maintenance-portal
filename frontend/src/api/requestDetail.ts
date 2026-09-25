import { api } from "./client"
import type { MaintenanceRequestResponse, CommentResponse, PhotoResponse } from "./types"

export const requestDetailApi = {
	get: (id: string) => api.get<MaintenanceRequestResponse>(`/requests/${id}`),
	updateStatus: (id: string, status?: string, priority?: string) =>
		api.patch<MaintenanceRequestResponse>(`/requests/${id}/status`, { status, priority }),

	getComments: (id: string) => api.get<CommentResponse[]>(`/requests/${id}/comments`),
	addComment: (id: string, body: string) =>
		api.post<CommentResponse>(`/requests/${id}/comments`, { body }),

	getPhotos: (id: string) => api.get<PhotoResponse[]>(`/requests/${id}/photos`),
	presignPhoto: (id: string, contentType: string, sizeBytes: number) =>
		api.post<{ key: string; uploadUrl: string; contentType: string; expiresInSeconds: number }>(
			`/requests/${id}/photos/presign`, { contentType, sizeBytes }
		),
	confirmPhoto: (id: string, key: string) =>
		api.post<PhotoResponse>(`/requests/${id}/photos/confirm`, { key }),
}
