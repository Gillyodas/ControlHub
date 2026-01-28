import { useEffect } from "react"
import { loadAuth } from "@/auth/storage"
import { jwtDecode } from "jwt-decode"

/**
 * Check if JWT token is expired or expiring soon
 */
function isTokenExpired(token: string, bufferMs = 60 * 1000): boolean {
    try {
        const decoded = jwtDecode<{ exp: number }>(token)
        return decoded.exp * 1000 < Date.now() + bufferMs
    } catch {
        return true
    }
}

/**
 * Hook to refresh token when tab becomes visible again
 * Handles case where token expires while user is on another tab
 */
export function useVisibilityRefresh(onRefreshNeeded: () => Promise<void>) {
    useEffect(() => {
        const handleVisibilityChange = async () => {
            if (document.visibilityState !== "visible") return

            const auth = loadAuth()
            if (!auth?.accessToken) return

            if (isTokenExpired(auth.accessToken)) {
                console.log("[visibility] Tab active, token expired, refreshing...")
                try {
                    await onRefreshNeeded()
                } catch (error) {
                    console.warn("[visibility] Refresh failed:", error)
                }
            }
        }

        document.addEventListener("visibilitychange", handleVisibilityChange)
        return () => document.removeEventListener("visibilitychange", handleVisibilityChange)
    }, [onRefreshNeeded])
}
