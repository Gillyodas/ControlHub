import { useState, useCallback } from "react"

interface UseAsyncState<T> {
  data: T | null
  error: Error | null
  isLoading: boolean
}

interface UseAsyncReturn<T, Args extends unknown[]> extends UseAsyncState<T> {
  execute: (...args: Args) => Promise<T | null>
  reset: () => void
}

export function useAsync<T, Args extends unknown[]>(
  asyncFunction: (...args: Args) => Promise<T>
): UseAsyncReturn<T, Args> {
  const [state, setState] = useState<UseAsyncState<T>>({
    data: null,
    error: null,
    isLoading: false,
  })

  const execute = useCallback(
    async (...args: Args): Promise<T | null> => {
      setState({ data: null, error: null, isLoading: true })
      try {
        const data = await asyncFunction(...args)
        setState({ data, error: null, isLoading: false })
        return data
      } catch (error) {
        setState({ data: null, error: error as Error, isLoading: false })
        return null
      }
    },
    [asyncFunction]
  )

  const reset = useCallback(() => {
    setState({ data: null, error: null, isLoading: false })
  }, [])

  return {
    ...state,
    execute,
    reset,
  }
}
