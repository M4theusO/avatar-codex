import { Link } from 'react-router'
import type { ElementSummary } from '../../types/content'
import type { Ranking } from '../../types/ranking'
import { formatXp } from '../../utils/ranking'
import styles from './Ranking.module.css'

interface MyPositionProps {
  ranking: Ranking
  /** Elemento do filtro atual (para explicar por que o usuário não aparece). */
  filterElement: ElementSummary | undefined
  userElement: ElementSummary | null
}

/** Posição de quem consulta, ou o motivo de não aparecer neste ranking (sem calcular nada aqui). */
export default function MyPosition({ ranking, filterElement, userElement }: MyPositionProps) {
  const { currentUser, entries } = ranking

  if (currentUser) {
    return (
      <p className={styles.myPosition}>
        Sua posição: <strong>{currentUser.position}º</strong> de {entries.totalItems} ·{' '}
        {formatXp(currentUser.experience)}
      </p>
    )
  }

  if (filterElement && !userElement) {
    return (
      <p className={styles.myPosition}>
        Você ainda não tem um elemento.{' '}
        <Link to="/element-test" className={styles.link}>
          Faça o teste de elemento
        </Link>{' '}
        para entrar no ranking de {filterElement.name}.
      </p>
    )
  }

  if (filterElement && userElement?.id !== filterElement.id) {
    return (
      <p className={styles.myPosition}>
        Seu elemento é {userElement?.name}: você aparece no ranking de {userElement?.name}, não no
        de {filterElement.name}.
      </p>
    )
  }

  return (
    <p className={styles.myPosition}>
      {ranking.period === 'month'
        ? 'Você ainda não ganhou XP este mês. Descubra personagens, faça quizzes e desafios para entrar.'
        : 'Ganhe XP explorando o universo para entrar no ranking.'}
    </p>
  )
}
