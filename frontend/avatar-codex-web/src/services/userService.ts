import type { CharacterListItem, PagedResult } from '../types/content'
import type {
  Dashboard,
  Discovery,
  DiscoveryResult,
  ExperienceTransaction,
  UserAchievement,
  UserStatistics,
} from '../types/progress'
import { api } from './api'

export async function discoverCharacter(id: number): Promise<DiscoveryResult> {
  const { data } = await api.post<DiscoveryResult>(`/api/characters/${id}/discovery`)
  return data
}

export async function discoverNation(id: number): Promise<DiscoveryResult> {
  const { data } = await api.post<DiscoveryResult>(`/api/nations/${id}/discovery`)
  return data
}

export async function addFavorite(characterId: number): Promise<void> {
  await api.post(`/api/characters/${characterId}/favorite`)
}

export async function removeFavorite(characterId: number): Promise<void> {
  await api.delete(`/api/characters/${characterId}/favorite`)
}

export async function getFavorites(page: number): Promise<PagedResult<CharacterListItem>> {
  const { data } = await api.get<PagedResult<CharacterListItem>>('/api/users/me/favorites', {
    params: { page },
  })
  return data
}

export async function getDiscoveries(pageSize: number): Promise<PagedResult<Discovery>> {
  const { data } = await api.get<PagedResult<Discovery>>('/api/users/me/discoveries', {
    params: { pageSize },
  })
  return data
}

export async function getExperience(page: number): Promise<PagedResult<ExperienceTransaction>> {
  const { data } = await api.get<PagedResult<ExperienceTransaction>>('/api/users/me/experience', {
    params: { page, pageSize: 10 },
  })
  return data
}

export async function getStatistics(): Promise<UserStatistics> {
  const { data } = await api.get<UserStatistics>('/api/users/me/statistics')
  return data
}

export async function getAchievements(): Promise<UserAchievement[]> {
  const { data } = await api.get<UserAchievement[]>('/api/users/me/achievements')
  return data
}

export async function getDashboard(): Promise<Dashboard> {
  const { data } = await api.get<Dashboard>('/api/users/me/dashboard')
  return data
}
