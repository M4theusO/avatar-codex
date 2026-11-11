import type { PagedResult } from '../types/content'
import type { FollowUser } from '../types/profile'
import { api } from './api'

export const FOLLOW_PAGE_SIZE = 12

/** Segue o usuário (idempotente). Privado ou inexistente → 404; a si mesmo ou a conta demo → 422. */
export async function followUser(username: string): Promise<void> {
  await api.put(`/api/users/${encodeURIComponent(username)}/follow`)
}

/** Deixa de seguir (sempre 204). */
export async function unfollowUser(username: string): Promise<void> {
  await api.delete(`/api/users/${encodeURIComponent(username)}/follow`)
}

export async function getFollowing(page: number): Promise<PagedResult<FollowUser>> {
  const { data } = await api.get<PagedResult<FollowUser>>('/api/users/me/following', {
    params: { page, pageSize: FOLLOW_PAGE_SIZE },
  })
  return data
}

export async function getFollowers(page: number): Promise<PagedResult<FollowUser>> {
  const { data } = await api.get<PagedResult<FollowUser>>('/api/users/me/followers', {
    params: { page, pageSize: FOLLOW_PAGE_SIZE },
  })
  return data
}
