import { describe, expect, it } from 'vitest'
import { hasActiveFilters, parseCharacterQuery } from './characterQuery'
import { getInitials } from './labels'
import { getPageItems } from './pagination'
import { getApiError } from './apiError'
import { safeRedirect } from './redirect'
import { parseTimelineFilters, timelineCharacters, timelineFiltersToParams } from './timeline'
import { comparePairToParams, parseComparePair } from './compare'
import { apiError } from '../test/fixtures'

describe('parseCharacterQuery', () => {
  it('lê filtros válidos da URL', () => {
    const query = parseCharacterQuery(
      new URLSearchParams(
        'page=2&search=zuko&nationId=4&type=bender&status=Alive&sort=nation&order=desc',
      ),
    )
    expect(query).toEqual({
      page: 2,
      search: 'zuko',
      nationId: 4,
      elementId: undefined,
      type: 'bender',
      status: 'Alive',
      seriesId: undefined,
      sort: 'nation',
      order: 'desc',
    })
  })

  it('descarta valores inválidos em vez de enviá-los à API', () => {
    const query = parseCharacterQuery(
      new URLSearchParams('page=abc&nationId=-1&elementId=1.5&type=wizard&status=alive&sort=age'),
    )
    expect(query.page).toBe(1)
    expect(query.nationId).toBeUndefined()
    expect(query.elementId).toBeUndefined()
    expect(query.type).toBeUndefined()
    expect(query.status).toBeUndefined()
    expect(query.sort).toBeUndefined()
  })

  it('ignora busca só com espaços', () => {
    expect(parseCharacterQuery(new URLSearchParams('search=%20%20')).search).toBeUndefined()
  })
})

describe('hasActiveFilters', () => {
  it('não considera página e ordenação como filtros', () => {
    expect(hasActiveFilters({ page: 3, sort: 'name', order: 'desc' })).toBe(false)
    expect(hasActiveFilters({ elementId: 2 })).toBe(true)
  })
})

describe('getPageItems', () => {
  it('mostra todas as páginas quando são poucas', () => {
    expect(getPageItems(2, 4)).toEqual([1, 2, 3, 4])
  })

  it('usa reticências nas lacunas', () => {
    expect(getPageItems(5, 10)).toEqual([1, 'gap', 4, 5, 6, 'gap', 10])
    expect(getPageItems(1, 10)).toEqual([1, 2, 'gap', 10])
  })
})

describe('safeRedirect', () => {
  it.each([
    ['/characters?type=avatar', '/characters?type=avatar'],
    [null, '/dashboard'],
    ['', '/dashboard'],
    ['https://evil.example.com', '/dashboard'],
    ['//evil.example.com', '/dashboard'],
    ['/\\evil.example.com', '/dashboard'],
  ])('%s → %s', (value, expected) => {
    expect(safeRedirect(value)).toBe(expected)
  })
})

describe('getApiError', () => {
  it('usa a mensagem e os erros por campo da API', () => {
    const details = getApiError(
      apiError(400, 'Os dados enviados são inválidos.', [
        { field: 'email', message: 'Email inválido.' },
        { field: 'email', message: 'Segunda mensagem ignorada.' },
      ]),
    )
    expect(details).toEqual({
      message: 'Os dados enviados são inválidos.',
      fieldErrors: { email: 'Email inválido.' },
    })
  })

  it('não repassa mensagens de erro 500', () => {
    expect(getApiError(apiError(500, 'NullReferenceException at …')).message).toBe(
      'Ocorreu um erro inesperado. Tente novamente mais tarde.',
    )
  })
})

describe('getInitials', () => {
  it.each([
    ['Aang', 'A'],
    ['Asami Sato', 'AS'],
    ['Toph Beifong', 'TB'],
    ['  Avatar   Kyoshi ', 'AK'],
  ])('%s → %s', (name, initials) => {
    expect(getInitials(name)).toBe(initials)
  })
})

describe('linha do tempo', () => {
  it('lê só ids positivos da URL e faz o caminho de volta', () => {
    const filters = parseTimelineFilters(new URLSearchParams('eraId=3&characterId=abc'))
    expect(filters).toEqual({ eraId: 3 })
    expect(parseTimelineFilters(new URLSearchParams('eraId=0&characterId=-2'))).toEqual({})
    expect(timelineFiltersToParams({ eraId: 3, characterId: 13 })).toEqual({
      eraId: '3',
      characterId: '13',
    })
  })

  it('lista os personagens da linha do tempo sem repetir, por nome', () => {
    const character = (id: number, name: string) => ({ id, name, image: null, element: null })
    const event = (id: number, characters: ReturnType<typeof character>[]) => ({
      id,
      title: `Evento ${id}`,
      description: '-',
      dateLabel: '-',
      image: null,
      characters,
    })

    const result = timelineCharacters([
      {
        id: 1,
        name: 'Era',
        description: '-',
        events: [event(1, [character(13, 'Zuko'), character(1, 'Aang')])],
      },
      {
        id: 2,
        name: 'Era 2',
        description: '-',
        events: [event(2, [character(13, 'Zuko'), character(7, 'Ícaro')])],
      },
    ])

    expect(result.map((c) => c.name)).toEqual(['Aang', 'Ícaro', 'Zuko'])
    expect(timelineCharacters(undefined)).toEqual([])
  })
})

describe('comparação', () => {
  it('lê os dois ids da URL e descarta valores inválidos ou repetidos', () => {
    expect(parseComparePair(new URLSearchParams('a=1&b=13'))).toEqual({ a: 1, b: 13 })
    expect(parseComparePair(new URLSearchParams('a=0&b=1.5'))).toEqual({})
    expect(parseComparePair(new URLSearchParams('a=7&b=7'))).toEqual({ a: 7 })
    expect(comparePairToParams({ b: 13 }).toString()).toBe('b=13')
  })
})
