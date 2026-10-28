import { useState } from 'react'
import type { ElementSummary, Image } from '../../types/content'
import { elementStyle, getInitials } from '../../utils/labels'
import styles from './CharacterAvatar.module.css'
import ElementIcon from './ElementIcon'

interface CharacterAvatarProps {
  name: string
  image: Image | null
  element?: ElementSummary | null
  size?: 'sm' | 'md' | 'lg'
}

/**
 * Imagem do personagem dentro de um anel na cor do elemento. Sem imagem (DA-04),
 * mostra as iniciais sobre o símbolo do elemento; elas também ficam por baixo
 * da imagem, aparecendo enquanto ela carrega ou se o link falhar.
 * É decorativo: o nome sempre aparece ao lado.
 */
export default function CharacterAvatar({
  name,
  image,
  element,
  size = 'md',
}: CharacterAvatarProps) {
  const [failedUrl, setFailedUrl] = useState<string | null>(null)
  const showImage = image !== null && image.url !== failedUrl

  return (
    <div
      className={`${styles.avatar} ${styles[size]}`}
      style={elementStyle(element?.color)}
      aria-hidden="true"
    >
      {element && <ElementIcon icon={element.icon} className={styles.emblem} />}
      <span className={styles.initials}>{getInitials(name)}</span>
      {showImage && (
        <img
          className={styles.image}
          src={image.url}
          alt=""
          loading="lazy"
          decoding="async"
          onError={() => setFailedUrl(image.url)}
        />
      )}
    </div>
  )
}
