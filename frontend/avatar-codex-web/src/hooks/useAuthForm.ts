import { useState, type ChangeEvent, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import type { SessionResult, User } from '../types/auth'
import { getApiError } from '../utils/apiError'
import { safeRedirect } from '../utils/redirect'
import { useAchievementToasts } from './useAchievementToasts'
import { useToast } from './useToast'

/**
 * Estado comum dos formulários de login e cadastro. A validação é toda do backend:
 * os erros por campo da resposta 400 aparecem ao lado de cada input.
 */
export function useAuthForm<T extends Record<string, string>>(
  initialValues: T,
  submit: (values: T) => Promise<SessionResult>,
  successMessage: (user: User) => string,
) {
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const { showToast } = useToast()
  const announceAchievements = useAchievementToasts()

  const [values, setValues] = useState(initialValues)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const [formError, setFormError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)

  const redirectParam = params.get('redirect')

  function bind(name: keyof T & string) {
    return {
      name,
      value: values[name],
      error: fieldErrors[name],
      onChange: (event: ChangeEvent<HTMLInputElement>) =>
        setValues((current) => ({ ...current, [name]: event.target.value })),
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (submitting) return

    setSubmitting(true)
    setFormError(undefined)
    setFieldErrors({})
    try {
      const { user, reward } = await submit(values)
      showToast(successMessage(user))
      // O XP do "Primeiro passo" já vem no `user`; aqui só o anúncio da conquista.
      if (reward) announceAchievements(reward.achievementsUnlocked)
      navigate(safeRedirect(redirectParam), { replace: true })
    } catch (error) {
      const details = getApiError(error)
      setFieldErrors(details.fieldErrors)
      setFormError(details.message)
      setSubmitting(false)
    }
  }

  return { bind, handleSubmit, formError, submitting, redirectParam }
}
