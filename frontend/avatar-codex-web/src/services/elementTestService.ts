import type { ElementTest, ElementTestAnswer, ElementTestResult } from '../types/elementTest'
import { api } from './api'

export async function getElementTest(): Promise<ElementTest> {
  const { data } = await api.get<ElementTest>('/api/element-test')
  return data
}

export async function submitElementTest(answers: ElementTestAnswer[]): Promise<ElementTestResult> {
  const { data } = await api.post<ElementTestResult>('/api/element-test/submissions', { answers })
  return data
}
