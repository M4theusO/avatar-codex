import { useMutation } from '@tanstack/react-query'
import { Star } from 'lucide-react'
import { useState } from 'react'
import { useToast } from '../../hooks/useToast'
import { useInvalidateUserContent } from '../../hooks/useUserData'
import { addFavorite, removeFavorite } from '../../services/userService'
import styles from './FavoriteButton.module.css'

interface FavoriteButtonProps {
  characterId: number
  name: string
  isFavorite: boolean
  /** `icon` nos cards; `full` (ícone + texto) no detalhe. */
  variant?: 'icon' | 'full'
}

/** Atualização otimista: muda na hora e volta ao estado anterior se a API falhar. */
export default function FavoriteButton({
  characterId,
  name,
  isFavorite,
  variant = 'icon',
}: FavoriteButtonProps) {
  const { showToast } = useToast()
  const invalidate = useInvalidateUserContent()
  const [pressed, setPressed] = useState(isFavorite)

  // Quando o servidor responde com outro valor (refetch), ele prevalece.
  const [serverValue, setServerValue] = useState(isFavorite)
  if (isFavorite !== serverValue) {
    setServerValue(isFavorite)
    setPressed(isFavorite)
  }

  const mutation = useMutation({
    mutationFn: (favorite: boolean) =>
      favorite ? addFavorite(characterId) : removeFavorite(characterId),
    onMutate: (favorite) => setPressed(favorite),
    onError: (_error, favorite) => {
      setPressed(!favorite)
      showToast('Não foi possível atualizar seus favoritos. Tente novamente.', 'error')
    },
    onSuccess: (_data, favorite) => {
      showToast(
        favorite ? '✓ Personagem adicionado aos favoritos.' : 'Personagem removido dos favoritos.',
        favorite ? 'success' : 'info',
      )
      invalidate()
    },
  })

  return (
    <button
      type="button"
      className={`${styles.button} ${styles[variant]}`}
      onClick={() => mutation.mutate(!pressed)}
      disabled={mutation.isPending}
      aria-pressed={pressed}
      aria-label={variant === 'icon' ? `Favoritar ${name}` : undefined}
    >
      <Star size={variant === 'icon' ? 18 : 20} aria-hidden="true" className={styles.star} />
      {variant === 'full' && (pressed ? 'Favoritado' : 'Favoritar')}
    </button>
  )
}
