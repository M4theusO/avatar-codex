export type HealthStatus = 'Healthy' | 'Degraded' | 'Unhealthy'

export interface HealthReport {
  status: HealthStatus
  checks: Record<string, HealthStatus>
}
