import { useMutation, useQuery } from '@tanstack/react-query'
import { getElementTest, submitElementTest } from '../services/elementTestService'
import type { ElementTestAnswer } from '../types/elementTest'
import { useAuth } from './useAuth'
import { useInvalidateUserContent } from './useUserData'

export function useElementTest() {
  return useQuery({
    queryKey: ['element-test'],
    queryFn: getElementTest,
    // As perguntas não mudam durante o teste.
    staleTime: Infinity,
  })
}

export function useSubmitElementTest() {
  const { updateUser } = useAuth()
  const invalidate = useInvalidateUserContent()
  return useMutation({
    mutationFn: (answers: ElementTestAnswer[]) => submitElementTest(answers),
    onSuccess: ({ element }) => {
      // O elemento calculado pelo backend passa a valer no perfil, na navbar e no painel.
      const { id, name, color, icon } = element
      updateUser({ element: { id, name, color, icon } })
      invalidate()
    },
  })
}
