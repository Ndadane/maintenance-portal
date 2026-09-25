import { Routes, Route, Navigate } from "react-router-dom"
import { useAuth } from "./auth/AuthContext"
import { ProtectedRoute } from "./auth/ProtectedRoute"
import { LoginPage } from "./pages/LoginPage"
import { RegisterPage } from "./pages/RegisterPage"
import { LandlordDashboard } from "./pages/LandlordDashboard"
import { TenantView } from "./pages/TenantView"
import { RequestDetailPage } from "./pages/RequestDetailPage"
import type { ReactNode } from "react"

function Placeholder({ text }: { text: string }) {
	return <p className="p-6 text-ink">{text}</p>
}

function Home() {
	const { user, isLandlord } = useAuth()
	if (!user) return <Navigate to="/login" replace />
	return <Navigate to={isLandlord ? "/landlord" : "/tenant"} replace />
}

function Header() {
	const { user, logout } = useAuth()
	if (!user) return null
	return (
		<header className="flex items-center justify-between bg-brand px-4 py-3">
			<span className="font-medium text-white">Maintenance Portal</span>
			<div className="flex items-center gap-3 text-sm text-brand-soft">
				<span>{user.fullName}</span>
				<button onClick={logout} className="text-white hover:underline">Sign out</button>
			</div>
		</header>
	)
}

function RequireAnyRole({ children }: { children: ReactNode }) {
	const { user } = useAuth()
	if (!user) return <Navigate to="/login" replace />
	return <>{children}</>
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
