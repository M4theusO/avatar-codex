import type { ComparePair } from '../types/content'

function positiveId(value: string | null): number | undefined {
  if (!value || !/^\d+$/.test(value)) return undefined
  const id = Number(value)
  return id > 0 ? id : undefined
}

/** Lê ?a=&b= da URL descartando valores inválidos; o mesmo personagem nos dois lados vale só para `a`. */
export function parseComparePair(params: URLSearchParams): ComparePair {
  const a = positiveId(params.get('a'))
  const b = positiveId(params.get('b'))
  return { a, b: b === a ? undefined : b }
}

export function comparePairToParams({ a, b }: ComparePair): URLSearchParams {
  const params = new URLSearchParams()
  if (a !== undefined) params.set('a', String(a))
  if (b !== undefined) params.set('b', String(b))
  return params
}
