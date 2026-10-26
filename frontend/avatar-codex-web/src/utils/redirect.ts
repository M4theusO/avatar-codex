export const DEFAULT_AFTER_LOGIN = '/profile'

/**
 * Aceita só caminhos internos em `?redirect=`, para que um link malicioso
 * não mande o usuário para outro site depois do login (open redirect).
 */
export function safeRedirect(value: string | null | undefined): string {
  if (!value || !value.startsWith('/') || value.startsWith('//') || value.startsWith('/\\')) {
    return DEFAULT_AFTER_LOGIN
  }
  return value
}

export function loginPath(redirectTo: string): string {
  return `/login?redirect=${encodeURIComponent(redirectTo)}`
}
