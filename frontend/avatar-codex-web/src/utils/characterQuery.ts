import type {
  CharacterQuery,
  CharacterSort,
  CharacterStatus,
  CharacterType,
  SortOrder,
} from '../types/content'

const types: CharacterType[] = ['avatar', 'bender', 'nonBender']
const statuses: CharacterStatus[] = ['Alive', 'Deceased', 'Unknown']
const sorts: CharacterSort[] = ['name', 'nation', 'element']
const orders: SortOrder[] = ['asc', 'desc']

/** Filtros que reduzem os resultados (página e ordenação não contam). */
export const filterKeys = ['search', 'nationId', 'elementId', 'type', 'status', 'seriesId'] as const

function positiveInt(value: string | null): number | undefined {
  if (value === null || !/^\d+$/.test(value)) return undefined
  const number = Number(value)
  return number > 0 ? number : undefined
}

function oneOf<T extends string>(value: string | null, allowed: T[]): T | undefined {
  return allowed.find((item) => item === value)
}

/** Lê a query string da URL, descartando valores inválidos em vez de quebrar a página. */
export function parseCharacterQuery(params: URLSearchParams): CharacterQuery {
  const search = params.get('search')?.trim()
  return {
    page: positiveInt(params.get('page')) ?? 1,
    search: search || undefined,
    nationId: positiveInt(params.get('nationId')),
    elementId: positiveInt(params.get('elementId')),
    type: oneOf(params.get('type'), types),
    status: oneOf(params.get('status'), statuses),
    seriesId: positiveInt(params.get('seriesId')),
    sort: oneOf(params.get('sort'), sorts),
    order: oneOf(params.get('order'), orders),
  }
}

export function hasActiveFilters(query: CharacterQuery): boolean {
  return filterKeys.some((key) => query[key] !== undefined)
}
