import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { deleteAccount, logout } from '../../services/authService'
import { apiError, mockSession } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

async function openDeleteDialog() {
  const user = userEvent.setup()
  await user.click(await screen.findByRole('button', { name: 'Excluir conta' }))
  const dialog = screen.getByRole('dialog', { name: 'Excluir conta?' })
  return { user, dialog }
}

describe('ProfilePage · excluir conta', () => {
  it('confirma com a senha, encerra a sessão e volta para a Home', async () => {
    mockSession()
    vi.mocked(deleteAccount).mockResolvedValue()
    const { router } = renderRoute('/profile')
    const { user, dialog } = await openDeleteDialog()

    const confirm = within(dialog).getByRole('button', { name: 'Excluir definitivamente' })
    expect(confirm).toBeDisabled()
    await user.type(within(dialog).getByLabelText('Senha'), 'Senha123')
    await user.click(confirm)

    expect(deleteAccount).toHaveBeenCalledWith('Senha123')
    await waitFor(() => expect(router.state.location.pathname).toBe('/'))
    expect(logout).toHaveBeenCalled()
    expect(await screen.findByText('Sua conta foi excluída.')).toBeInTheDocument()
  })

  it('senha errada: mostra o erro no campo e continua logado', async () => {
    mockSession()
    vi.mocked(deleteAccount).mockRejectedValue(
      apiError(400, 'Dados inválidos.', [{ field: 'password', message: 'Senha incorreta.' }]),
    )
    const { router } = renderRoute('/profile')
    const { user, dialog } = await openDeleteDialog()

    await user.type(within(dialog).getByLabelText('Senha'), 'Errada123')
    await user.click(within(dialog).getByRole('button', { name: 'Excluir definitivamente' }))

    expect(await within(dialog).findByText('Senha incorreta.')).toBeInTheDocument()
    expect(within(dialog).getByLabelText('Senha')).toHaveAttribute('aria-invalid', 'true')
    expect(logout).not.toHaveBeenCalled()
    expect(router.state.location.pathname).toBe('/profile')
  })

  it('conta de demonstração: mostra a mensagem da API', async () => {
    mockSession()
    vi.mocked(deleteAccount).mockRejectedValue(
      apiError(422, 'A conta de demonstração é compartilhada e não pode ser excluída.'),
    )
    renderRoute('/profile')
    const { user, dialog } = await openDeleteDialog()

    await user.type(within(dialog).getByLabelText('Senha'), 'Avatar123')
    await user.click(within(dialog).getByRole('button', { name: 'Excluir definitivamente' }))

    expect(await within(dialog).findByRole('alert')).toHaveTextContent(
      'A conta de demonstração é compartilhada e não pode ser excluída.',
    )
  })

  it('cancelar fecha o modal sem excluir', async () => {
    mockSession()
    renderRoute('/profile')
    const { user, dialog } = await openDeleteDialog()

    await user.click(within(dialog).getByRole('button', { name: 'Cancelar' }))

    expect(dialog).not.toHaveAttribute('open')
    expect(deleteAccount).not.toHaveBeenCalled()
  })
})
