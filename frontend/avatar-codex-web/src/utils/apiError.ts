import { isAxiosError } from 'axios'

export function isNotFoundError(error: unknown): boolean {
  return isAxiosError(error) && error.response?.status === 404
}

export function isClientError(error: unknown): boolean {
  const status = isAxiosError(error) ? error.response?.status : undefined
  return status !== undefined && status >= 400 && status < 500
}
