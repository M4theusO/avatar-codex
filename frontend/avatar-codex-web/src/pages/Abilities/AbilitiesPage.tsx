import { Sparkles } from 'lucide-react'
import { useId } from 'react'
import AbilityCard from '../../components/abilities/AbilityCard'
import ElementIcon from '../../components/common/ElementIcon'
import PageHeader from '../../components/common/PageHeader'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useAbilities } from '../../hooks/useContent'
import { type AbilityGroup, groupAbilitiesByElement } from '../../utils/abilities'
import { elementStyle } from '../../utils/labels'
import styles from './AbilitiesPage.module.css'

export default function AbilitiesPage() {
  const { data, isPending, isError, refetch } = useAbilities()

  return (
    <>
      <title>Habilidades · Avatar Codex</title>
      <PageHeader
        title="Habilidades"
        description="Técnicas de dobra e habilidades especiais do mundo de Avatar, e quem as domina."
      />

      {isPending ? (
        <div className={styles.grid} aria-busy="true">
          <span className="visually-hidden" role="status">
            Carregando habilidades…
          </span>
          {Array.from({ length: 6 }, (_, index) => (
            <Skeleton key={index} height="9rem" />
          ))}
        </div>
      ) : isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : data.length === 0 ? (
        <EmptyState icon={Sparkles} title="Nenhuma habilidade cadastrada ainda" />
      ) : (
        <div className={styles.groups}>
          {groupAbilitiesByElement(data).map((group) => (
            <AbilityGroupSection key={group.element?.id ?? 'none'} group={group} />
          ))}
        </div>
      )}
    </>
  )
}

function AbilityGroupSection({ group }: { group: AbilityGroup }) {
  const titleId = useId()
  const { element, abilities } = group

  return (
    <section
      className={styles.group}
      aria-labelledby={titleId}
      style={elementStyle(element?.color)}
    >
      <h2 id={titleId} className={styles.groupTitle}>
        <ElementIcon icon={element?.icon} size={22} className={styles.groupIcon} />
        {element ? element.name : 'Sem elemento'}
      </h2>
      <ul className={styles.grid}>
        {abilities.map((ability) => (
          <li key={ability.id}>
            <AbilityCard ability={ability} />
          </li>
        ))}
      </ul>
    </section>
  )
}
