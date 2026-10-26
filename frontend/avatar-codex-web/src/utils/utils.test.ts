import { describe, expect, it } from 'vitest'
import { hasActiveFilters, parseCharacterQuery } from './characterQuery'
import { getInitials } from './labels'
import { getPageItems } from './pagination'
import { getApiError } from './apiError'
import { safeRedirect } from './redirect'
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
    [null, '/profile'],
    ['', '/profile'],
    ['https://evil.example.com', '/profile'],
    ['//evil.example.com', '/profile'],
    ['/\\evil.example.com', '/profile'],
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
