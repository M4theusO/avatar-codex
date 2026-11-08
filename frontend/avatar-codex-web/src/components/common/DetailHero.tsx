import type { ReactNode } from 'react'
import type { ElementSummary, Image } from '../../types/content'
import { elementStyle } from '../../utils/labels'
import styles from './DetailHero.module.css'
import ElementIcon from './ElementIcon'

interface DetailHeroProps {
  title: string
  description: string
  element: ElementSummary | null
  /** Imagem própria (símbolo da nação, ilustração da habilidade); sem ela, o ícone do elemento. */
  image: Image | null
  children?: ReactNode
}

/** Cabeçalho das páginas de detalhe de nação e habilidade, no tom do elemento. */
export default function DetailHero({
  title,
  description,
  element,
  image,
  children,
}: DetailHeroProps) {
  return (
    <header className={styles.hero} style={elementStyle(element?.color)}>
      <div className={styles.symbol}>
        {image ? <img src={image.url} alt="" /> : <ElementIcon icon={element?.icon} size={40} />}
      </div>
      <div className={styles.text}>
        <h1 className={styles.title}>{title}</h1>
        <p className={styles.description}>{description}</p>
        {children}
      </div>
    </header>
  )
}
