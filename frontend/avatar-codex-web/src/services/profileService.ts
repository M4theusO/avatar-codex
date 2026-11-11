import type { PublicProfile } from '../types/profile'
import { api } from './api'

/** Perfil público pelo nome de usuário (sem diferenciar maiúsculas/minúsculas). Privado → 404. */
export async function getPublicProfile(username: string): Promise<PublicProfile> {
  const { data } = await api.get<PublicProfile>(`/api/users/${encodeURIComponent(username)}`)
  return data
}
