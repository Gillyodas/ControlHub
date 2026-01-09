const API_BASE: string = import.meta.env.VITE_API_BASE_URL ?? ""

type ProblemDetails = {
  title?: string
  status?: number
  detail?: string
  extensions?: {
    code?: string
  }
}

async function readErrorMessage(res: Response) {
  try {
    const json = (await res.json()) as ProblemDetails
    return json?.detail || json?.title || `Request failed (${res.status})`
  } catch {
    return `Request failed (${res.status})`
  }
}

export async function fetchJson<T>(
  path: string,
  options?: {
    method?: string
    body?: unknown
    accessToken?: string
    headers?: Record<string, string>
  }
): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: options?.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      ...(options?.accessToken ? { Authorization: `Bearer ${options.accessToken}` } : {}),
      ...(options?.headers ?? {}),
    },
    body: options?.body ? JSON.stringify(options.body) : undefined,
  })

  if (!res.ok) {
    throw new Error(await readErrorMessage(res))
  }

  if (res.status === 204) {
    return undefined as T
  }

  return (await res.json()) as T
}

export async function fetchVoid(
  path: string,
  options?: {
    method?: string
    body?: unknown
    accessToken?: string
    headers?: Record<string, string>
  }
): Promise<void> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: options?.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      ...(options?.accessToken ? { Authorization: `Bearer ${options.accessToken}` } : {}),
      ...(options?.headers ?? {}),
    },
    body: options?.body ? JSON.stringify(options.body) : undefined,
  })

  if (!res.ok) {
    throw new Error(await readErrorMessage(res))
  }
}
