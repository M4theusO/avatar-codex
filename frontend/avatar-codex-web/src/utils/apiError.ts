import { isAxiosError } from 'axios'

export function isNotFoundError(error: unknown): boolean {
  return isAxiosError(error) && error.response?.status === 404
}

export function isClientError(error: unknown): boolean {
  const status = isAxiosError(error) ? error.response?.status : undefined
  return status !== undefined && status >= 400 && status < 500
}

export interface ApiErrorDetails {
  message: string
  /** Mensagens por campo (`errors` do formato padrão da API, docs/api.md §3). */
  fieldErrors: Record<string, string>
}

interface ErrorBody {
  message?: string
  errors?: { field: string; message: string }[]
}

/** Extrai mensagens amigáveis de um erro da API; nunca expõe detalhes técnicos. */
export function getApiError(error: unknown): ApiErrorDetails {
  if (!isAxiosError<ErrorBody>(error) || !error.response) {
    return { message: 'Não foi possível conectar ao servidor. Tente novamente.', fieldErrors: {} }
  }

  const body = error.response.data
  const fieldErrors: Record<string, string> = {}
  for (const { field, message } of body?.errors ?? []) {
    fieldErrors[field] ??= message
  }

  return {
    message:
      error.response.status >= 500 || !body?.message
        ? 'Ocorreu um erro inesperado. Tente novamente mais tarde.'
        : body.message,
    fieldErrors,
  }
}
