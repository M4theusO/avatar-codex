import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router'
import type { CharacterQuery } from '../types/content'
import { parseCharacterQuery } from '../utils/characterQuery'

/** Estado da listagem de personagens guardado na URL (compartilhável e com "voltar"). */
export function useCharacterQueryParams() {
  const [params, setParams] = useSearchParams()
  const query = useMemo(() => parseCharacterQuery(params), [params])

  const update = useCallback(
    (changes: Partial<CharacterQuery>, options?: { replace?: boolean }) => {
      setParams(
        (previous) => {
          const next = new URLSearchParams(previous)
          for (const [key, value] of Object.entries(changes)) {
            if (value === undefined || value === '') next.delete(key)
            else next.set(key, String(value))
          }
          // Qualquer mudança de filtro volta para a primeira página.
          if (!('page' in changes)) next.delete('page')
          if (next.get('page') === '1') next.delete('page')
          return next
        },
        { replace: options?.replace },
      )
    },
    [setParams],
  )

  const clear = useCallback(() => setParams({}), [setParams])

  return { query, update, clear }
}
