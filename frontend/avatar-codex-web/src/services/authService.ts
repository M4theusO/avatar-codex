import type { AuthResponse, LoginData, RegisterData, SessionResult, User } from '../types/auth'
import { api, refreshSession, setAccessToken } from './api'

function startSession(data: AuthResponse): SessionResult {
  setAccessToken(data.accessToken)
  return { user: data.user, reward: data.reward ?? null }
}

export async function login(data: LoginData) {
  const response = await api.post<AuthResponse>('/api/auth/login', data)
  return startSession(response.data)
}

export async function register(data: RegisterData) {
  const response = await api.post<AuthResponse>('/api/auth/register', data)
  return startSession(response.data)
}

/** Recupera a sessão ao abrir/recarregar a página, usando o cookie de refresh. */
export async function restoreSession() {
  return (await refreshSession()).user
}

export async function logout() {
  try {
    await api.post('/api/auth/logout')
  } finally {
    // Mesmo se a API falhar, a sessão local termina.
    setAccessToken(null)
  }
}

/** Exclui a conta (confirmando a senha). A sessão local termina com o logout logo depois. */
export async function deleteAccount(password: string) {
  await api.delete('/api/users/me', { data: { password } })
}

/** Troca o nome de usuário. Devolve o usuário atualizado. */
export async function updateProfile(username: string) {
  return (await api.patch<User>('/api/users/me', { username })).data
}

/** Torna o perfil privado (só você vê) ou visível para quem está logado. Devolve o usuário. */
export async function updatePrivacy(isProfilePrivate: boolean) {
  return (await api.put<User>('/api/users/me/privacy', { isProfilePrivate })).data
}

/** Envia a foto de perfil (substitui a anterior). Devolve o usuário com o novo `avatarUrl`. */
export async function uploadAvatar(image: Blob) {
  const form = new FormData()
  form.append('file', image, 'avatar.jpg')
  return (await api.put<User>('/api/users/me/avatar', form)).data
}

/** Remove a foto de perfil (volta para as iniciais). */
export async function removeAvatar() {
  return (await api.delete<User>('/api/users/me/avatar')).data
}
