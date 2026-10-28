import { keepPreviousData, useQuery } from '@tanstack/react-query'
import {
  compareCharacters,
  getCharacter,
  getCharacters,
  getElements,
  getEras,
  getNation,
  getNations,
  getSeries,
  getTimeline,
} from '../services/contentService'
import type { CharacterQuery, TimelineFilters } from '../types/content'
import { useAuth } from './useAuth'

/**
 * Personagens vêm com isFavorite/isDiscovered de quem está logado: o cache é separado por
 * usuário e só busca depois que a sessão foi verificada (evita buscar duas vezes ao abrir a página).
 */
function useViewer() {
  const { status, user } = useAuth()
  return { ready: status !== 'loading', key: user?.id ?? 'anonymous' }
}

export function useCharacters(query: CharacterQuery) {
  const viewer = useViewer()
  return useQuery({
    queryKey: ['characters', viewer.key, query],
    queryFn: () => getCharacters(query),
    enabled: viewer.ready,
    // Mantém a página anterior visível enquanto a próxima carrega.
    placeholderData: keepPreviousData,
  })
}

export function useCharacter(id: number) {
  const viewer = useViewer()
  return useQuery({
    queryKey: ['characters', viewer.key, id],
    queryFn: () => getCharacter(id),
    enabled: viewer.ready && Number.isInteger(id) && id > 0,
  })
}

export function useNations() {
  return useQuery({ queryKey: ['nations'], queryFn: getNations })
}

export function useNation(id: number) {
  const viewer = useViewer()
  return useQuery({
    queryKey: ['nations', viewer.key, id],
    queryFn: () => getNation(id),
    enabled: viewer.ready && Number.isInteger(id) && id > 0,
  })
}

export function useElements() {
  return useQuery({ queryKey: ['elements'], queryFn: getElements })
}

export function useSeries() {
  return useQuery({ queryKey: ['series'], queryFn: getSeries })
}

export function useTimeline(filters: TimelineFilters) {
  return useQuery({
    queryKey: ['timeline', filters],
    queryFn: () => getTimeline(filters),
    placeholderData: keepPreviousData,
  })
}

export function useEras() {
  return useQuery({ queryKey: ['eras'], queryFn: getEras })
}

export function useComparison(a: number | undefined, b: number | undefined) {
  return useQuery({
    queryKey: ['comparison', a, b],
    queryFn: () => compareCharacters(a!, b!),
    enabled: a !== undefined && b !== undefined && a !== b,
  })
}
