import type { ReactNode } from "react"
import { Navigate } from "react-router-dom"
import { useAuth } from "./AuthContext"

export function ProtectedRoute({ role, children }: { role: "Landlord" | "Tenant"; children: ReactNode }) {
	const { user, isLandlord, isTenant } = useAuth()

	if (!user) return <Navigate to="/login" replace />
	if (role === "Landlord" && !isLandlord) return <Navigate to="/" replace />
	if (role === "Tenant" && !isTenant) return <Navigate to="/" replace />

	return <>{children}</>
}
