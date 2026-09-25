import { api } from "./client"
import type { PropertyResponse, UnitResponse, MaintenanceRequestResponse } from "./types"

export interface AssignTenantResponse {
	id: string
	unitId: string
	tenantId: string
	tenantEmail: string
	moveInDate: string
	moveOutDate: string | null
	tenantWasAutoCreated: boolean
	temporaryPassword?: string
}

export const landlordApi = {
	getProperties: () => api.get<PropertyResponse[]>("/properties"),
	createProperty: (name: string, address: string) =>
		api.post<PropertyResponse>("/properties", { name, address }),

	getUnits: (propertyId: string) => api.get<UnitResponse[]>(`/properties/${propertyId}/units`),
	createUnit: (propertyId: string, unitLabel: string) =>
		api.post<UnitResponse>(`/properties/${propertyId}/units`, { unitLabel }),

	assignTenant: (unitId: string, tenantEmail: string, moveInDate: string) =>
		api.post<AssignTenantResponse>(`/units/${unitId}/tenants`, { tenantEmail, moveInDate }),

	getRequests: (propertyId?: string) =>
		api.get<MaintenanceRequestResponse[]>(`/requests${propertyId ? `?propertyId=${propertyId}` : ""}`),

	updateStatus: (requestId: string, status?: string, priority?: string) =>
		api.patch<MaintenanceRequestResponse>(`/requests/${requestId}/status`, { status, priority }),
}
