import { Routes, Route, Navigate } from "react-router-dom"
import { useAuth } from "./auth/AuthContext"
import { ProtectedRoute } from "./auth/ProtectedRoute"
import { LoginPage } from "./pages/LoginPage"
import { RegisterPage } from "./pages/RegisterPage"
import { LandlordDashboard } from "./pages/LandlordDashboard"
import { TenantView } from "./pages/TenantView"
import { RequestDetailPage } from "./pages/RequestDetailPage"
import type { ReactNode } from "react"

function RequireAnyRole({ children }: { children: ReactNode }) {
	const { user } = useAuth()
	if (!user) return <Navigate to="/login" replace />
	return <>{children}</>
}

function Home() {
	const { user, isLandlord } = useAuth()
	if (!user) return <Navigate to="/login" replace />
	return <Navigate to={isLandlord ? "/landlord" : "/tenant"} replace />
}

function Header() {
	const { user, isLandlord, logout } = useAuth()
	if (!user) return null
	return (
		<header className="flex flex-col gap-2 bg-brand px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
			<div className="flex items-center gap-2">
				<span className="font-medium text-white">Maintenance Portal</span>
				<span className="rounded-full bg-brand-dark px-2 py-0.5 text-xs font-medium text-brand-soft">
					{isLandlord ? "Landlord" : "Tenant"}
				</span>
			</div>
			<div className="flex items-center gap-3 text-sm text-brand-soft">
				<span>{user.fullName}</span>
				<button onClick={logout} className="text-white hover:underline">Sign out</button>
			</div>
		</header>
	)
}

export default function App() {
	return (
		<div className="min-h-screen">
			<Header />
			<Routes>
				<Route path="/login" element={<LoginPage />} />
				<Route path="/register" element={<RegisterPage />} />
				<Route path="/" element={<Home />} />
				<Route
					path="/landlord"
					element={<ProtectedRoute role="Landlord"><LandlordDashboard /></ProtectedRoute>}
				/>
				<Route
					path="/tenant"
					element={<ProtectedRoute role="Tenant"><TenantView /></ProtectedRoute>}
				/>
				<Route
					path="/requests/:id"
					element={<RequireAnyRole><RequestDetailPage /></RequireAnyRole>}
				/>
			</Routes>
		</div>
	)
}
