import http from './http'

export interface Device {
  deviceId: string
  deviceName: string
  description?: string
  deviceType: string
  status: 'Online' | 'Offline' | 'Error' | 'Maintenance'
  location?: string
  firmwareVersion?: string
  hardwareVersion?: string
  lastKnownIP?: string
  lastSeen?: string
  createdAt: string
  updatedAt: string
  capabilities?: string
  configuration?: string
  batteryLevel?: number
  temperature?: number
  signalStrength?: number
  manufacturer?: string
  model?: string
  serialNumber?: string
  macAddress?: string
  port?: number
  isActive: boolean
  isRegistered: boolean
  heartbeatInterval: number
  lastHeartbeat?: string
  lastError?: string
  lastErrorTime?: string
  errorCount: number
}

export interface DeviceEvent {
  eventId: string
  deviceId: string
  eventType: string
  message?: string
  data?: string
  timestamp: string
  severity: 'Info' | 'Warning' | 'Error' | 'Critical'
  isProcessed: boolean
}

export interface DeviceRegistrationRequest {
  deviceName: string
  description?: string
  deviceType: string
  location?: string
  firmwareVersion?: string
  hardwareVersion?: string
  capabilities?: string
  configuration?: string
  manufacturer?: string
  model?: string
  serialNumber?: string
  macAddress?: string
  heartbeatInterval?: number
}

export interface DeviceHeartbeatRequest {
  deviceId: string
  status?: string
  batteryLevel?: number
  temperature?: number
  signalStrength?: number
  lastKnownIP?: string
  data?: string
}

export interface UpdateStatusRequest {
  status: string
}

export const deviceService = {
  // Register a new device
  register(request: DeviceRegistrationRequest) {
    return http.post<{ success: boolean; deviceId: string; message: string }>('/device/register', request)
  },

  // Send heartbeat
  heartbeat(request: DeviceHeartbeatRequest) {
    return http.post<{ success: boolean; message: string; timestamp: string }>('/device/heartbeat', request)
  },

  // Get all devices
  getAll() {
    return http.get<Device[]>('/device')
  },

  // Get device by ID
  getById(deviceId: string) {
    return http.get<Device>(`/device/${deviceId}`)
  },

  // Get online devices
  getOnline() {
    return http.get<Device[]>('/device/online')
  },

  // Get offline devices
  getOffline() {
    return http.get<Device[]>('/device/offline')
  },

  // Update device status
  updateStatus(deviceId: string, request: UpdateStatusRequest) {
    return http.put<{ success: boolean; message: string }>(`/device/${deviceId}/status`, request)
  },

  // Delete device
  delete(deviceId: string) {
    return http.delete<{ success: boolean; message: string }>(`/device/${deviceId}`)
  },

  // Get device events
  getEvents(deviceId: string, limit: number = 50) {
    return http.get<DeviceEvent[]>(`/device/${deviceId}/events?limit=${limit}`)
  },

  // Get device status
  getStatus(deviceId: string) {
    return http.get<{ deviceId: string; isOnline: boolean; timestamp: string }>(`/device/${deviceId}/status`)
  },

  // Cleanup offline devices
  cleanup(timeoutMinutes: number = 5) {
    return http.post<{ success: boolean; message: string }>(`/device/cleanup?timeoutMinutes=${timeoutMinutes}`)
  }
}
