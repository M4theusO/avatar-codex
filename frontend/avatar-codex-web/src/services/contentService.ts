import type {
  CharacterComparison,
  CharacterDetails,
  CharacterListItem,
  CharacterQuery,
  Element,
  Era,
  Nation,
  NationDetails,
  PagedResult,
  Series,
  TimelineEra,
  TimelineFilters,
} from '../types/content'
import { api } from './api'

export async function getCharacters(
  query: CharacterQuery,
): Promise<PagedResult<CharacterListItem>> {
  const { data } = await api.get<PagedResult<CharacterListItem>>('/api/characters', {
    params: query,
  })
  return data
}

export async function getCharacter(id: number): Promise<CharacterDetails> {
  const { data } = await api.get<CharacterDetails>(`/api/characters/${id}`)
  return data
}

export async function getNations(): Promise<Nation[]> {
  const { data } = await api.get<Nation[]>('/api/nations')
  return data
}

export async function getNation(id: number): Promise<NationDetails> {
  const { data } = await api.get<NationDetails>(`/api/nations/${id}`)
  return data
}

export async function getElements(): Promise<Element[]> {
  const { data } = await api.get<Element[]>('/api/elements')
  return data
}

export async function getSeries(): Promise<Series[]> {
  const { data } = await api.get<Series[]>('/api/series')
  return data
}

export async function getTimeline(filters: TimelineFilters): Promise<TimelineEra[]> {
  const { data } = await api.get<TimelineEra[]>('/api/timeline', { params: filters })
  return data
}

export async function getEras(): Promise<Era[]> {
  const { data } = await api.get<Era[]>('/api/eras')
  return data
}

export async function compareCharacters(a: number, b: number): Promise<CharacterComparison> {
  const { data } = await api.get<CharacterComparison>('/api/characters/compare', {
    params: { ids: `${a},${b}` },
  })
  return data
}
