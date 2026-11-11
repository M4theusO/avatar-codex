import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import {
  removeAvatar,
  updatePrivacy,
  updateProfile,
  uploadAvatar,
} from '../../services/authService'
import { apiError, mockSession, user as userFixture } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import { resizeAvatar } from '../../utils/userAvatar'

// O canvas não existe no jsdom: a redução da foto é simulada.
vi.mock('../../utils/userAvatar', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../../utils/userAvatar')>()),
  resizeAvatar: vi.fn(),
}))

const resized = new Blob(['jpeg'], { type: 'image/jpeg' })

async function openProfile(signed = userFixture()) {
  mockSession(signed)
  renderRoute('/profile')
  const section = (await screen.findByRole('heading', { name: 'Editar perfil' })).closest(
    'section',
  )!
  return { section: within(section as HTMLElement), user: userEvent.setup({ applyAccept: false }) }
}

describe('ProfilePage · editar perfil', () => {
  it('salva o novo nome de usuário e atualiza o perfil', async () => {
    vi.mocked(updateProfile).mockResolvedValue(userFixture({ username: 'melon_lord' }))
    const { section, user } = await openProfile()

    const save = section.getByRole('button', { name: 'Salvar nome' })
    expect(save).toBeDisabled()
    const field = section.getByLabelText('Nome de usuário')
    await user.clear(field)
    await user.type(field, '  melon_lord ')
    await user.click(save)

    expect(updateProfile).toHaveBeenCalledWith('melon_lord')
    expect(await screen.findByRole('heading', { level: 1, name: 'melon_lord' })).toBeInTheDocument()
    expect(await screen.findByText('Nome de usuário atualizado.')).toBeInTheDocument()
  })

  it('nome em uso: mostra a mensagem da API no campo', async () => {
    vi.mocked(updateProfile).mockRejectedValue(
      apiError(409, 'Este nome de usuário já está em uso.'),
    )
    const { section, user } = await openProfile()

    const field = section.getByLabelText('Nome de usuário')
    await user.clear(field)
    await user.type(field, 'katara')
    await user.click(section.getByRole('button', { name: 'Salvar nome' }))

    expect(await section.findByText('Este nome de usuário já está em uso.')).toBeInTheDocument()
    expect(field).toHaveAttribute('aria-invalid', 'true')
    expect(screen.getByRole('heading', { level: 1, name: 'toph' })).toBeInTheDocument()
  })

  it('envia a foto reduzida e mostra a nova imagem', async () => {
    vi.mocked(resizeAvatar).mockResolvedValue(resized)
    vi.mocked(uploadAvatar).mockResolvedValue(userFixture({ avatarUrl: '/api/avatars/abc' }))
    const { section, user } = await openProfile()
    const file = new File(['png'], 'eu.png', { type: 'image/png' })

    expect(section.getByRole('button', { name: 'Enviar foto' })).toBeInTheDocument()
    await user.upload(section.getByLabelText('Escolher foto de perfil'), file)

    expect(resizeAvatar).toHaveBeenCalledWith(file)
    expect(uploadAvatar).toHaveBeenCalledWith(resized)
    expect(await screen.findByText('Foto atualizada.')).toBeInTheDocument()
    expect(section.getByRole('button', { name: 'Trocar foto' })).toBeInTheDocument()
    await waitFor(() =>
      expect(document.querySelector('img[src$="/api/avatars/abc"]')).not.toBeNull(),
    )
  })

  it('arquivo que não é imagem aceita: avisa sem enviar', async () => {
    const { section, user } = await openProfile()

    await user.upload(
      section.getByLabelText('Escolher foto de perfil'),
      new File(['gif'], 'a.gif', { type: 'image/gif' }),
    )

    expect(await section.findByRole('alert')).toHaveTextContent(
      'Escolha uma imagem JPEG, PNG ou WebP.',
    )
    expect(uploadAvatar).not.toHaveBeenCalled()
  })

  it('erro da API no envio aparece na seção', async () => {
    vi.mocked(resizeAvatar).mockResolvedValue(resized)
    vi.mocked(uploadAvatar).mockRejectedValue(
      apiError(422, 'A conta de demonstração é compartilhada e a foto dela não pode ser alterada.'),
    )
    const { section, user } = await openProfile()

    await user.upload(
      section.getByLabelText('Escolher foto de perfil'),
      new File(['jpg'], 'a.jpg', { type: 'image/jpeg' }),
    )

    expect(await section.findByRole('alert')).toHaveTextContent(
      'A conta de demonstração é compartilhada e a foto dela não pode ser alterada.',
    )
  })

  it('remove a foto e volta para as iniciais', async () => {
    vi.mocked(removeAvatar).mockResolvedValue(userFixture())
    const { section, user } = await openProfile(userFixture({ avatarUrl: '/api/avatars/abc' }))

    await user.click(section.getByRole('button', { name: 'Remover foto' }))

    expect(removeAvatar).toHaveBeenCalled()
    expect(await screen.findByText('Foto removida.')).toBeInTheDocument()
    expect(section.queryByRole('button', { name: 'Remover foto' })).not.toBeInTheDocument()
    expect(document.querySelector('img[src$="/api/avatars/abc"]')).toBeNull()
  })

  it('torna o perfil privado e mostra o link para o perfil público', async () => {
    vi.mocked(updatePrivacy).mockResolvedValue(userFixture({ isProfilePrivate: true }))
    const { section, user } = await openProfile()

    const toggle = section.getByRole('switch', { name: 'Perfil privado' })
    expect(toggle).not.toBeChecked()
    expect(section.getByRole('link', { name: 'Ver meu perfil público' })).toHaveAttribute(
      'href',
      '/users/toph',
    )
    await user.click(toggle)

    expect(updatePrivacy).toHaveBeenCalledWith(true)
    expect(await screen.findByText('Seu perfil agora é privado.')).toBeInTheDocument()
    expect(toggle).toBeChecked()
    expect(section.getByText(/Só você vê seu perfil público/)).toBeInTheDocument()
  })

  it('conta demo: a recusa da API aparece e a chave não muda', async () => {
    vi.mocked(updatePrivacy).mockRejectedValue(
      apiError(422, 'A conta de demonstração é compartilhada e o perfil dela continua visível.'),
    )
    const { section, user } = await openProfile()

    const toggle = section.getByRole('switch', { name: 'Perfil privado' })
    await user.click(toggle)

    expect(await section.findByRole('alert')).toHaveTextContent('o perfil dela continua visível')
    expect(toggle).not.toBeChecked()
  })
})
