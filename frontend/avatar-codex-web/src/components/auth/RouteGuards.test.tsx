import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { logout } from '../../services/authService'
import { mockSession, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

describe('rotas protegidas e sessão', () => {
  it('visitante em /profile vai para o login com redirect', async () => {
    const { router } = renderRoute('/profile')

    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
    expect(router.state.location.search).toBe('?redirect=%2Fprofile')
    expect(await screen.findByRole('heading', { name: 'Entrar' })).toBeInTheDocument()
  })

  it('com sessão recuperada, /profile mostra os dados do usuário', async () => {
    mockSession(user({ username: 'korra', email: 'korra@example.com', level: 3 }))
    renderRoute('/profile')

    expect(await screen.findByRole('heading', { level: 1, name: 'korra' })).toBeInTheDocument()
    expect(screen.getByText('korra@example.com')).toBeInTheDocument()
    expect(screen.getByText('26 de outubro de 2026')).toBeInTheDocument()
  })

  it('navbar mostra entrar/criar conta para visitantes', async () => {
    renderRoute('/')

    const nav = screen.getByRole('navigation', { name: 'Principal' })
    expect(await within(nav).findByRole('link', { name: 'Entrar' })).toBeInTheDocument()
    expect(within(nav).getByRole('link', { name: 'Criar conta' })).toBeInTheDocument()
  })

  it('logout pela navbar encerra a sessão e volta para o início', async () => {
    mockSession()
    const actions = userEvent.setup()
    const { router } = renderRoute('/profile')

    const nav = await screen.findByRole('navigation', { name: 'Principal' })
    expect(await within(nav).findByRole('link', { name: /toph/ })).toHaveAttribute(
      'href',
      '/profile',
    )

    await actions.click(within(nav).getByRole('button', { name: 'Sair' }))

    expect(logout).toHaveBeenCalledOnce()
    await waitFor(() => expect(router.state.location.pathname).toBe('/'))
    expect(await screen.findByText('Você saiu da sua conta.')).toBeInTheDocument()
    expect(await within(nav).findByRole('link', { name: 'Entrar' })).toBeInTheDocument()
  })
})
