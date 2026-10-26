import type {
  CharacterDetails,
  CharacterListItem,
  CharacterQuery,
  Element,
  Nation,
  NationDetails,
  PagedResult,
  Series,
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
