// src/services/schedulerService.ts
import http from './http'

export interface Scheduler {
  schedulerId: string // Guid => string
  schedulerName: string
  schedulerDescription: string
  schedulerPath: string
  active: boolean
  runAll: boolean
}

export interface SchedulerDay {
  schedulerDayId: string // Guid => string
  schedulerId: string // Guid => string
  day: number
  from: string // .NET string giữ nguyên
  to: string // .NET string giữ nguyên
  active: boolean
}

export interface CreateUpdateSchedulerDto {
  master?: Scheduler // nullable trong C# => optional trong TS
  detail?: SchedulerDay[] // List<T> => T[]
  editMode: number // int => number
}


export const schedulerService = {
  create(input: CreateUpdateSchedulerDto) {
    return http.post<boolean>('/tool', input)
  },

  update(id: string, input: CreateUpdateSchedulerDto) {
    return http.put<boolean>(`/tool`, input)
  },

  get() {
    return http.get<Scheduler[]>(`/tool/GetSchedulers`)
  },

  getDetail(id: string) {
    return http.get<SchedulerDay[]>(`/tool/GetSchedulerDays/${id}`)
  },

  getList() {
    return http.get<CreateUpdateSchedulerDto[]>('/tool/GetListScheduler', {})
  },
}
