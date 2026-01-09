import { useEffect, useCallback } from "react"
import { useAuth } from "@/auth/use-auth"
import { authApi } from "@/services/api"

const TOKEN_REFRESH_INTERVAL = 14 * 60 * 1000 // 14 minutes (tokens typically expire in 15 mins)

export function useTokenRefresh() {
  const { auth, updateAuth, signOut } = useAuth()

  const refreshToken = useCallback(async () => {
    if (!auth?.refreshToken || !auth?.accessToken || !auth?.accountId) {
      return
    }

    try {
      const result = await authApi.refreshAccessToken({
        refreshToken: auth.refreshToken,
        accID: String(auth.accountId),
        accessToken: auth.accessToken,
      })

      updateAuth({
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
      })

      console.log("Token refreshed successfully")
    } catch (error) {
      console.error("Failed to refresh token:", error)
      signOut()
    }
  }, [auth, updateAuth, signOut])

  useEffect(() => {
    if (!auth?.accessToken) {
      return
    }

    const interval = setInterval(refreshToken, TOKEN_REFRESH_INTERVAL)

    return () => clearInterval(interval)
  }, [auth?.accessToken, refreshToken])

  return { refreshToken }
}
