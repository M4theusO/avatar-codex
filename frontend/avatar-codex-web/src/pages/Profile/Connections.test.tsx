import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getFollowers, getFollowing } from '../../services/followService'
import { fire, mockSession, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { FollowUser } from '../../types/profile'

function person(overrides: Partial<FollowUser> = {}): FollowUser {
  return {
    username: 'zuko',
    avatarUrl: null,
    element: fire,
    level: 4,
    isProfilePrivate: false,
    followedAt: '2026-11-11T12:00:00Z',
    ...overrides,
  }
}

async function openConnections() {
  mockSession()
  renderRoute('/profile')
  const heading = await screen.findByRole('heading', { name: 'Conexões' })
  return within(heading.closest('section') as HTMLElement)
}

describe('ProfilePage · conexões', () => {
  it('lista quem você segue, com link só para perfis visíveis, e os totais nas abas', async () => {
    vi.mocked(getFollowing).mockResolvedValue(
      page([person(), person({ username: 'azula', isProfilePrivate: true, element: null })]),
    )
    vi.mocked(getFollowers).mockResolvedValue(page([person({ username: 'katara' })]))
    const section = await openConnections()

    const list = await section.findByRole('list', { name: 'Seguindo' })
    expect(within(list).getByRole('link', { name: 'zuko' })).toHaveAttribute('href', '/users/zuko')
    expect(within(list).getByText('Nv. 4 · Fogo')).toBeInTheDocument()
    expect(within(list).queryByRole('link', { name: 'azula' })).not.toBeInTheDocument()
    expect(within(list).getByText(/Perfil privado/)).toBeInTheDocument()
    const followingTab = section.getByRole('button', { name: /^Seguindo/ })
    expect(followingTab).toHaveAttribute('aria-pressed', 'true')
    expect(followingTab).toHaveTextContent('Seguindo 2')
    expect(section.getByRole('button', { name: /^Seguidores/ })).toHaveTextContent('Seguidores 1')

    await userEvent.setup().click(section.getByRole('button', { name: /^Seguidores/ }))

    const followers = await section.findByRole('list', { name: 'Seguidores' })
    expect(within(followers).getByRole('link', { name: 'katara' })).toBeInTheDocument()
  })

  it('sem conexões: convida a seguir alguém pelo ranking', async () => {
    const section = await openConnections()

    expect(await section.findByText('Você ainda não segue ninguém')).toBeInTheDocument()
    expect(section.getByRole('link', { name: 'Ver ranking' })).toHaveAttribute('href', '/ranking')

    await userEvent.setup().click(section.getByRole('button', { name: /Seguidores/ }))
    expect(await section.findByText('Ninguém segue você ainda')).toBeInTheDocument()
  })

  it('erro ao carregar: permite tentar de novo', async () => {
    vi.mocked(getFollowing).mockRejectedValue(new Error('offline'))
    const section = await openConnections()

    expect(await section.findByText('Não foi possível carregar a lista')).toBeInTheDocument()
    expect(section.getByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})
