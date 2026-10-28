import type { CharacterSummary, TimelineEra, TimelineFilters } from '../types/content'

/** Personagens que aparecem na linha do tempo (opções do filtro), sem repetição e por nome. */
export function timelineCharacters(eras: TimelineEra[] | undefined): CharacterSummary[] {
  const byId = new Map<number, CharacterSummary>()
  for (const event of eras?.flatMap((era) => era.events) ?? []) {
    for (const character of event.characters) byId.set(character.id, character)
  }
  return [...byId.values()].sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'))
}

function positiveInt(value: string | null): number | undefined {
  const number = Number(value)
  return Number.isInteger(number) && number > 0 ? number : undefined
}

/** Filtros a partir da URL (`?eraId=3&characterId=5`); valores inválidos são ignorados. */
export function parseTimelineFilters(params: URLSearchParams): TimelineFilters {
  const filters: TimelineFilters = {}
  const eraId = positiveInt(params.get('eraId'))
  const characterId = positiveInt(params.get('characterId'))
  if (eraId) filters.eraId = eraId
  if (characterId) filters.characterId = characterId
  return filters
}

export function timelineFiltersToParams(filters: TimelineFilters): Record<string, string> {
  const params: Record<string, string> = {}
  if (filters.eraId) params.eraId = String(filters.eraId)
  if (filters.characterId) params.characterId = String(filters.characterId)
  return params
}
