import { fetchJson } from "./client"
import type {
  UpdateUsernameRequest,
  UpdateUsernameResponse,
} from "./types"

export async function updateUsername(
  userId: string,
  req: UpdateUsernameRequest,
  accessToken: string
): Promise<UpdateUsernameResponse> {
  return fetchJson<UpdateUsernameResponse>(`/api/User/username/${userId}`, {
    method: "PATCH",
    body: req,
    accessToken,
  })
}
