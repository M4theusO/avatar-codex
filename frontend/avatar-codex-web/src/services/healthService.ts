import type { HealthReport } from '../types/health'
import { api } from './api'

export async function getHealth(): Promise<HealthReport> {
  // 503 também traz o relatório no corpo, então não é tratado como erro aqui.
  const response = await api.get<HealthReport>('/api/health', {
    validateStatus: (status) => status === 200 || status === 503,
  })
  return response.data
}
