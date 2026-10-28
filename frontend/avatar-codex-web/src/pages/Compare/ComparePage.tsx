import { ArrowLeftRight, Info, Scale, UserX } from 'lucide-react'
import { useSearchParams } from 'react-router'
import AttributeBars from '../../components/compare/AttributeBars'
import AttributeRadar from '../../components/compare/AttributeRadar'
import CharacterPicker from '../../components/compare/CharacterPicker'
import CompareHeader from '../../components/compare/CompareHeader'
import CompareInfoTable from '../../components/compare/CompareInfoTable'
import styles from '../../components/compare/Compare.module.css'
import { Button } from '../../components/common/Button'
import PageHeader from '../../components/common/PageHeader'
import Section from '../../components/common/Section'
import Skeleton from '../../components/common/Skeleton'
import { EmptyState, ErrorState } from '../../components/common/StatusMessage'
import { useComparison } from '../../hooks/useContent'
import type { ComparePair } from '../../types/content'
import { isNotFoundError } from '../../utils/apiError'
import { comparePairToParams, parseComparePair } from '../../utils/compare'

export default function ComparePage() {
  const [params, setParams] = useSearchParams()
  const pair = parseComparePair(params)
  const comparison = useComparison(pair.a, pair.b)

  function update(next: ComparePair) {
    setParams(comparePairToParams(next), { replace: true })
  }

  const ready = pair.a !== undefined && pair.b !== undefined

  return (
    <div className={styles.page}>
      <title>Comparar personagens · Avatar Codex</title>
      <PageHeader
        title="Comparar personagens"
        description="Escolha dois personagens para ver elemento, nação, habilidades e atributos lado a lado."
      />

      <div className={styles.pickers}>
        <CharacterPicker
          slot="a"
          label="Primeiro personagem"
          selectedId={pair.a}
          excludeId={pair.b}
          onSelect={(a) => update({ ...pair, a })}
          onClear={() => update({ b: pair.b })}
        />
        <CharacterPicker
          slot="b"
          label="Segundo personagem"
          selectedId={pair.b}
          excludeId={pair.a}
          onSelect={(b) => update({ ...pair, b })}
          onClear={() => update({ a: pair.a })}
        />
      </div>
      {ready && (
        <Button
          className={styles.swap}
          variant="ghost"
          onClick={() => update({ a: pair.b, b: pair.a })}
        >
          <ArrowLeftRight size={18} aria-hidden="true" />
          Inverter lados
        </Button>
      )}

      <p className={styles.notice} role="note">
        <Info size={18} aria-hidden="true" />
        Atributos são estimativas da comunidade do projeto, não informações canônicas.
      </p>

      {!ready ? (
        <EmptyState
          icon={Scale}
          title="Escolha dois personagens"
          description="Use as buscas acima. A comparação aparece assim que os dois lados estiverem escolhidos."
        />
      ) : comparison.isPending ? (
        <div className={styles.comparison} aria-busy="true">
          <span className="visually-hidden" role="status">
            Carregando a comparação…
          </span>
          <Skeleton height="12rem" />
          <Skeleton height="20rem" />
        </div>
      ) : comparison.isError ? (
        isNotFoundError(comparison.error) ? (
          <EmptyState
            icon={UserX}
            title="Personagem não encontrado"
            description="Um dos personagens do link não existe mais. Troque-o para continuar."
          />
        ) : (
          <ErrorState
            title="Não foi possível carregar a comparação"
            onRetry={() => comparison.refetch()}
          />
        )
      ) : (
        <div className={styles.comparison}>
          <CompareHeader characters={comparison.data.characters} />

          <Section title="Atributos">
            {comparison.data.attributes.some((a) => a.values.some((v) => v !== null)) ? (
              <div className={styles.charts}>
                <AttributeRadar {...comparison.data} />
                <AttributeBars {...comparison.data} />
              </div>
            ) : (
              <p className={styles.pickerMessage}>
                Ainda não há estimativas de atributos para esses personagens.
              </p>
            )}
          </Section>

          <Section title="Informações">
            <CompareInfoTable characters={comparison.data.characters} />
          </Section>
        </div>
      )}
    </div>
  )
}
