import type { RankingQuery } from '../types/ranking'

function positiveInt(value: string | null): number | undefined {
  const number = Number(value)
  return Number.isInteger(number) && number > 0 ? number : undefined
}

/** Filtros a partir da URL (`?period=month&element=3&page=2`); valores inválidos voltam ao padrão. */
export function parseRankingQuery(params: URLSearchParams): RankingQuery {
  const query: RankingQuery = {
    period: params.get('period') === 'month' ? 'month' : 'all',
    page: positiveInt(params.get('page')) ?? 1,
  }
  const elementId = positiveInt(params.get('element'))
  if (elementId) query.elementId = elementId
  return query
}

/** O padrão (geral, todos os elementos, página 1) não aparece na URL. */
export function rankingQueryToParams(query: RankingQuery): Record<string, string> {
  const params: Record<string, string> = {}
  if (query.period === 'month') params.period = 'month'
  if (query.elementId) params.element = String(query.elementId)
  if (query.page > 1) params.page = String(query.page)
  return params
}

const monthFormat = new Intl.DateTimeFormat('pt-BR', {
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})
const dayFormat = new Intl.DateTimeFormat('pt-BR', {
  day: 'numeric',
  month: 'long',
  timeZone: 'UTC',
})

/** "novembro de 2026 · recomeça em 1 de dezembro" (datas em UTC, como o backend). */
export function monthLabel(periodStart: string, periodEnd: string): string {
  return `${monthFormat.format(new Date(periodStart))} · recomeça em ${dayFormat.format(new Date(periodEnd))}`
}

const number = new Intl.NumberFormat('pt-BR')

export function formatXp(value: number): string {
  return `${number.format(value)} XP`
}
