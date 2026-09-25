import { createContext, useContext, useState, type ReactNode } from "react"
import { api } from "../api/client"
import type { AuthResponse } from "../api/types"

interface AuthUser {
	email: string
	fullName: string
	roles: string[]
}

interface AuthContextValue {
	user: AuthUser | null
	isLandlord: boolean
	isTenant: boolean
	login: (email: string, password: string) => Promise<void>
	register: (email: string, password: string, fullName: string, role: "Landlord" | "Tenant") => Promise<void>
	logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function loadUser(): AuthUser | null {
	const raw = localStorage.getItem("user")
	return raw ? (JSON.parse(raw) as AuthUser) : null
}

export function AuthProvider({ children }: { children: ReactNode }) {
	const [user, setUser] = useState<AuthUser | null>(loadUser)

	function store(res: AuthResponse) {
		localStorage.setItem("token", res.token)
		const authUser = { email: res.email, fullName: res.fullName, roles: res.roles }
		localStorage.setItem("user", JSON.stringify(authUser))
		setUser(authUser)
	}

	async function login(email: string, password: string) {
		const res = await api.post<AuthResponse>("/auth/login", { email, password })
		store(res)
	}

	async function register(email: string, password: string, fullName: string, role: "Landlord" | "Tenant") {
		const res = await api.post<AuthResponse>("/auth/register", { email, password, fullName, role })
		store(res)
	}

	function logout() {
		localStorage.removeItem("token")
		localStorage.removeItem("user")
		setUser(null)
	}

	const isLandlord = user?.roles.includes("Landlord") ?? false
	const isTenant = user?.roles.includes("Tenant") ?? false

	return (
		<AuthContext.Provider value={{ user, isLandlord, isTenant, login, register, logout }}>
			{children}
		</AuthContext.Provider>
	)
}

export function useAuth() {
	const ctx = useContext(AuthContext)
	if (!ctx) throw new Error("useAuth must be used inside AuthProvider")
	return ctx
}
