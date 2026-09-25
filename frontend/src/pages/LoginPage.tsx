import { useState } from "react"
import { useNavigate, Link } from "react-router-dom"
import { useAuth } from "../auth/AuthContext"
import { ApiError } from "../api/client"
import { FormShell, TextField } from "../components/FormShell"

export function LoginPage() {
	const { login, isLandlord } = useAuth()
	const navigate = useNavigate()
	const [email, setEmail] = useState("")
	const [password, setPassword] = useState("")
	const [error, setError] = useState<string | null>(null)
	const [loading, setLoading] = useState(false)

	async function handleSubmit(e: React.FormEvent) {
		e.preventDefault()
		setError(null)
		setLoading(true)
		try {
			await login(email, password)
			navigate(isLandlord ? "/landlord" : "/tenant")
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Something went wrong. Try again.")
		} finally {
			setLoading(false)
		}
	}

	return (
		<FormShell
			title="Log in"
			error={error}
			onSubmit={handleSubmit}
			footer={<>Need an account? <Link className="text-brand" to="/register">Register</Link></>}
		>
			<TextField label="Email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
			<TextField label="Password" type="password" required value={password} onChange={(e) => setPassword(e.target.value)} />
			<button
				type="submit"
				disabled={loading}
				className="mt-2 rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark disabled:opacity-60"
			>
				{loading ? "Logging in..." : "Log in"}
			</button>
		</FormShell>
	)
}
