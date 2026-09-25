import { api } from "./client"
import type { MaintenanceRequestResponse } from "./types"

export interface MyUnitResponse {
	assignmentId: string
	unitId: string
	unitLabel: string
	propertyId: string
	propertyName: string
	propertyAddress: string
	moveInDate: string
}

export const tenantApi = {
	getMyUnit: () => api.get<MyUnitResponse>("/my-unit"),
	getRequests: () => api.get<MaintenanceRequestResponse[]>("/requests"),
	createRequest: (title: string, description: string, category: string, priority: string) =>
		api.post<MaintenanceRequestResponse>("/requests", { title, description, category, priority }),
}
