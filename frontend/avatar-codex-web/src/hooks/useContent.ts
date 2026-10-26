import { keepPreviousData, useQuery } from '@tanstack/react-query'
import {
  getCharacter,
  getCharacters,
  getElements,
  getNation,
  getNations,
  getSeries,
} from '../services/contentService'
import type { CharacterQuery } from '../types/content'

export function useCharacters(query: CharacterQuery) {
  return useQuery({
    queryKey: ['characters', query],
    queryFn: () => getCharacters(query),
    // Mantém a página anterior visível enquanto a próxima carrega.
    placeholderData: keepPreviousData,
  })
}

export function useCharacter(id: number) {
  return useQuery({
    queryKey: ['characters', id],
    queryFn: () => getCharacter(id),
    enabled: Number.isInteger(id) && id > 0,
  })
}

export function useNations() {
  return useQuery({ queryKey: ['nations'], queryFn: getNations })
}

export function useNation(id: number) {
  return useQuery({
    queryKey: ['nations', id],
    queryFn: () => getNation(id),
    enabled: Number.isInteger(id) && id > 0,
  })
}

export function useElements() {
  return useQuery({ queryKey: ['elements'], queryFn: getElements })
}

export function useSeries() {
  return useQuery({ queryKey: ['series'], queryFn: getSeries })
}
