import type {
  CharacterDetails,
  CharacterListItem,
  ElementSummary,
  PagedResult,
} from '../types/content'

export const fire: ElementSummary = { id: 3, name: 'Fogo', color: '#B86B35', icon: 'flame' }

export function character(overrides: Partial<CharacterListItem> = {}): CharacterListItem {
  return {
    id: 13,
    name: 'Zuko',
    description: 'Príncipe banido da Nação do Fogo.',
    image: null,
    nation: { id: 4, name: 'Nação do Fogo' },
    element: fire,
    isBender: true,
    isAvatar: false,
    status: 'Alive',
    ...overrides,
  }
}

export function page<T>(items: T[], overrides: Partial<PagedResult<T>> = {}): PagedResult<T> {
  return {
    items,
    page: 1,
    pageSize: 12,
    totalItems: items.length,
    totalPages: items.length > 0 ? 1 : 0,
    ...overrides,
  }
}

export function characterDetails(overrides: Partial<CharacterDetails> = {}): CharacterDetails {
  return {
    ...character(),
    aliases: ['Lee', 'Espírito Azul'],
    biography: 'Banido pelo pai, passou anos caçando o Avatar.\n\nDepois se tornou Senhor do Fogo.',
    firstAppearance: 'O Garoto no Iceberg',
    bendingSubtypes: [{ id: 7, name: 'Dobra de relâmpago' }],
    abilities: [
      {
        id: 1,
        name: 'Redirecionar relâmpago',
        description: 'Desvia raios.',
        element: fire,
        image: null,
      },
    ],
    series: [{ id: 1, code: 'ATLA', name: 'Avatar: A Lenda de Aang' }],
    relationships: [
      {
        character: { id: 14, name: 'Iroh', image: null, element: fire },
        type: 'Mentor',
        description: 'Tio',
      },
    ],
    ...overrides,
  }
}
