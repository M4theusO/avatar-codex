import { AxiosError, AxiosHeaders } from 'axios'
import { vi } from 'vitest'
import { restoreSession } from '../services/authService'
import type { User } from '../types/auth'
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
    isFavorite: false,
    isDiscovered: false,
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

export function user(overrides: Partial<User> = {}): User {
  return {
    id: '4f1c2b7e-0000-4000-8000-000000000001',
    username: 'toph',
    email: 'toph@example.com',
    avatarUrl: null,
    element: null,
    level: 1,
    experiencePoints: 0,
    currentLevelXp: 0,
    nextLevelXp: 100,
    levelProgress: 0,
    createdAt: '2026-10-26T12:00:00Z',
    ...overrides,
  }
}

/** Simula um usuário com sessão ativa (o refresh da abertura da página funciona). */
export function mockSession(signedUser: User = user()) {
  vi.mocked(restoreSession).mockResolvedValue(signedUser)
  return signedUser
}

/** Erro da API no formato padrão (docs/api.md §3). */
export function apiError(
  status: number,
  message: string,
  errors: { field: string; message: string }[] = [],
) {
  const config = { headers: new AxiosHeaders() }
  return new AxiosError(message, 'ERR_BAD_REQUEST', config, null, {
    status,
    statusText: '',
    data: { status, message, errors },
    headers: {},
    config,
  })
}
