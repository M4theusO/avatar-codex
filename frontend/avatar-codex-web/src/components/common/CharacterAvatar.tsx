import type { Image } from '../../types/content'
import { elementStyle, getInitials } from '../../utils/labels'
import styles from './CharacterAvatar.module.css'

interface CharacterAvatarProps {
  name: string
  image: Image | null
  color?: string | null
  size?: 'sm' | 'md' | 'lg'
}

/**
 * Imagem do personagem ou, enquanto não houver imagem curada (DA-04),
 * um placeholder com as iniciais na cor do elemento. É decorativo:
 * o nome sempre aparece ao lado.
 */
export default function CharacterAvatar({ name, image, color, size = 'md' }: CharacterAvatarProps) {
  const className = `${styles.avatar} ${styles[size]}`

  if (image) {
    return <img className={className} src={image.url} alt="" loading="lazy" />
  }

  return (
    <div className={className} style={elementStyle(color)} aria-hidden="true">
      <span className={styles.initials}>{getInitials(name)}</span>
    </div>
  )
}
