import { useState } from "react"
import { useNavigate, Link } from "react-router-dom"
import { useAuth } from "../auth/AuthContext"
import { ApiError } from "../api/client"
import { FormShell, TextField } from "../components/FormShell"

export function RegisterPage() {
	const { register } = useAuth()
	const navigate = useNavigate()
	const [email, setEmail] = useState("")
	const [password, setPassword] = useState("")
	const [fullName, setFullName] = useState("")
	const [role, setRole] = useState<"Landlord" | "Tenant">("Landlord")
	const [error, setError] = useState<string | null>(null)
	const [loading, setLoading] = useState(false)

	async function handleSubmit(e: React.FormEvent) {
		e.preventDefault()
		setError(null)
		setLoading(true)
		try {
			await register(email, password, fullName, role)
			navigate(role === "Landlord" ? "/landlord" : "/tenant")
		} catch (err) {
			setError(err instanceof ApiError ? err.message : "Something went wrong. Try again.")
		} finally {
			setLoading(false)
		}
	}

	return (
		<FormShell
			title="Register"
			error={error}
			onSubmit={handleSubmit}
			footer={<>Already have an account? <Link className="text-brand" to="/login">Log in</Link></>}
		>
			<TextField label="Full name" required value={fullName} onChange={(e) => setFullName(e.target.value)} />
			<TextField label="Email" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
			<TextField label="Password" type="password" required minLength={8} value={password} onChange={(e) => setPassword(e.target.value)} />
			<label className="flex flex-col gap-1 text-sm text-ink">
				I am a
				<select
					value={role}
					onChange={(e) => setRole(e.target.value as "Landlord" | "Tenant")}
					className="rounded-md border border-line px-3 py-2 text-sm outline-none focus:border-brand"
				>
					<option value="Landlord">Landlord</option>
					<option value="Tenant">Tenant</option>
				</select>
			</label>
			<button
				type="submit"
				disabled={loading}
				className="mt-2 rounded-md bg-brand px-3 py-2 text-sm font-medium text-white hover:bg-brand-dark disabled:opacity-60"
			>
				{loading ? "Creating account..." : "Register"}
			</button>
		</FormShell>
	)
}
