import { ApiError } from "./client"

export function friendlyError(err: unknown, fallback = "Something went wrong. Please try again."): string {
	if (!(err instanceof ApiError)) return fallback
	switch (true) {
		case err.status === 401:
			return "Your session has expired. Please log in again."
		case err.status === 403:
			return "You don't have permission to do that."
		case err.status === 404:
			return "That couldn't be found. It may have been removed."
		case err.status === 409:
			return err.message || "That conflicts with existing data."
		case err.status === 400:
			return err.message || "Please check the form and try again."
		case err.status >= 500:
			return "The server had a problem. Please try again shortly."
		default:
			return err.message || fallback
	}
}
