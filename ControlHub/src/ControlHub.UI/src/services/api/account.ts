import { fetchVoid } from "./client"
import type {
  ChangePasswordRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
} from "./types"

export async function changePassword(
  userId: string,
  req: ChangePasswordRequest,
  accessToken: string
): Promise<void> {
  return fetchVoid(`/api/Account/change-password/${userId}`, {
    method: "POST",
    body: req,
    accessToken,
  })
}

export async function forgotPassword(req: ForgotPasswordRequest): Promise<void> {
  return fetchVoid("/api/Account/forgot-password", {
    method: "POST",
    body: req,
  })
}

export async function resetPassword(req: ResetPasswordRequest): Promise<void> {
  return fetchVoid("/api/Account/reset-password", {
    method: "POST",
    body: req,
  })
}
