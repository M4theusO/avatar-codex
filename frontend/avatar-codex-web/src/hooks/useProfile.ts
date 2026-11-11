import { useMutation } from '@tanstack/react-query'
import { removeAvatar, updatePrivacy, updateProfile, uploadAvatar } from '../services/authService'
import type { User } from '../types/auth'
import { resizeAvatar } from '../utils/userAvatar'
import { useAuth } from './useAuth'
import { useInvalidateUserContent } from './useUserData'

/** Depois de qualquer edição, o usuário da API passa a valer na navbar, no perfil e no ranking. */
function useApplyUser() {
  const { updateUser } = useAuth()
  const invalidate = useInvalidateUserContent()
  return (user: User) => {
    updateUser({
      username: user.username,
      avatarUrl: user.avatarUrl,
      isProfilePrivate: user.isProfilePrivate,
    })
    invalidate()
  }
}

export function useUpdateUsername() {
  const apply = useApplyUser()
  return useMutation({
    mutationFn: (username: string) => updateProfile(username),
    onSuccess: apply,
  })
}

export function useUpdatePrivacy() {
  const apply = useApplyUser()
  return useMutation({
    mutationFn: (isProfilePrivate: boolean) => updatePrivacy(isProfilePrivate),
    onSuccess: apply,
  })
}

/** Reduz a foto no navegador e envia; a nova substitui a anterior no backend. */
export function useUploadAvatar() {
  const apply = useApplyUser()
  return useMutation({
    mutationFn: async (file: File) => uploadAvatar(await resizeAvatar(file)),
    onSuccess: apply,
  })
}

export function useRemoveAvatar() {
  const apply = useApplyUser()
  return useMutation({ mutationFn: () => removeAvatar(), onSuccess: apply })
}
