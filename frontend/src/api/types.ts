export interface AuthResponse {
	token: string
	email: string
	fullName: string
	roles: string[]
}

export interface PropertyResponse {
	id: string
	name: string
	address: string
}

export interface UnitResponse {
	id: string
	propertyId: string
	unitLabel: string
}

export interface MaintenanceRequestResponse {
	id: string
	title: string
	description: string
	category: string
	priority: string
	status: string
	createdAt: string
	resolvedAt: string | null
	unitId: string
	unitLabel: string
	propertyId: string
	propertyName: string
}

export interface CommentResponse {
	id: string
	authorId: string
	authorEmail: string
	authorRole: string
	body: string
	createdAt: string
}

export interface PhotoResponse {
	id: string
	contentType: string
	sizeBytes: number
	uploadedAt: string
	downloadUrl: string
}
