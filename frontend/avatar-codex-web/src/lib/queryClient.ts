import { QueryClient } from '@tanstack/react-query'
import { isClientError } from '../utils/apiError'

export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Conteúdo da enciclopédia muda pouco.
        staleTime: 5 * 60 * 1000,
        refetchOnWindowFocus: false,
        // 4xx (ex.: 404) não melhora com nova tentativa.
        retry: (failureCount, error) => !isClientError(error) && failureCount < 2,
      },
    },
  })
}
