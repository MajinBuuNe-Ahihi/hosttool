<template>
  <div class="device-management">
    <div class="card shadow-sm">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">
          <i class="fa fa-microchip me-2"></i>Device Management
        </h5>
        <div class="d-flex gap-2">
          <button class="btn btn-success btn-sm" @click="refreshDevices">
            <i class="fa fa-refresh me-1"></i>Refresh
          </button>
          <button class="btn btn-primary btn-sm" @click="showAddDeviceModal">
            <i class="fa fa-plus me-1"></i>Add Device
          </button>
          <button class="btn btn-warning btn-sm" @click="cleanupOfflineDevices">
            <i class="fa fa-trash me-1"></i>Cleanup Offline
          </button>
        </div>
      </div>

      <div class="card-body">
        <!-- Device Statistics -->
        <div class="row mb-3">
          <div class="col-md-3">
            <div class="card bg-success text-white">
              <div class="card-body text-center">
                <h4>{{ onlineDevicesCount }}</h4>
                <small>Online Devices</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-warning text-white">
              <div class="card-body text-center">
                <h4>{{ offlineDevicesCount }}</h4>
                <small>Offline Devices</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-info text-white">
              <div class="card-body text-center">
                <h4>{{ totalDevicesCount }}</h4>
                <small>Total Devices</small>
              </div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="card bg-secondary text-white">
              <div class="card-body text-center">
                <h4>{{ errorDevicesCount }}</h4>
                <small>Error Devices</small>
              </div>
            </div>
          </div>
        </div>

        <!-- Device List -->
        <div class="table-responsive">
          <table class="table table-striped table-hover align-middle">
            <thead class="table-light">
              <tr>
                <th>Status</th>
                <th>Device Name</th>
                <th>Type</th>
                <th>Location</th>
                <th>Last Seen</th>
                <th>IP Address</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="device in devices" :key="device.deviceId">
                <td>
                  <span 
                    class="badge" 
                    :class="getStatusBadgeClass(device.status)"
                  >
                    <i :class="getStatusIcon(device.status)" class="me-1"></i>
                    {{ device.status }}
                  </span>
                </td>
                <td>
                  <div>
                    <strong>{{ device.deviceName }}</strong>
                    <br>
                    <small class="text-muted">{{ device.manufacturer }} {{ device.model }}</small>
                  </div>
                </td>
                <td>
                  <span class="badge bg-secondary">{{ device.deviceType }}</span>
                </td>
                <td>{{ device.location || 'N/A' }}</td>
                <td>
                  <div v-if="device.lastSeen">
                    {{ formatDateTime(device.lastSeen) }}
                    <br>
                    <small class="text-muted">{{ getTimeAgo(device.lastSeen) }}</small>
                  </div>
                  <span v-else class="text-muted">Never</span>
                </td>
                <td>
                  <code>{{ device.lastKnownIP || 'N/A' }}</code>
                </td>
                <td>
                  <div class="btn-group">
                    <button 
                      class="btn btn-sm btn-outline-info" 
                      @click="viewDeviceDetails(device)"
                      title="View Details"
                    >
                      <i class="fa fa-eye"></i>
                    </button>
                    <button 
                      class="btn btn-sm btn-outline-warning" 
                      @click="editDevice(device)"
                      title="Edit Device"
                    >
                      <i class="fa fa-edit"></i>
                    </button>
                    <button 
                      class="btn btn-sm btn-outline-danger" 
                      @click="deleteDevice(device)"
                      title="Delete Device"
                    >
                      <i class="fa fa-trash"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="devices.length === 0" class="text-center py-4">
          <i class="fa fa-microchip fa-3x text-muted mb-3"></i>
          <p class="text-muted">No devices found. Add a device to get started.</p>
        </div>
      </div>
    </div>

    <!-- Add Device Modal -->
    <div class="modal fade show" tabindex="-1" style="display: block" v-if="showAddModal">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header bg-light">
            <h5 class="modal-title">Add New Device</h5>
            <button type="button" class="btn-close" @click="showAddModal = false"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="addDevice">
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Device Name *</label>
                    <input v-model="newDevice.deviceName" type="text" class="form-control" required />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Device Type *</label>
                    <select v-model="newDevice.deviceType" class="form-select" required>
                      <option value="">Select Type</option>
                      <option value="IoT">IoT Device</option>
                      <option value="Sensor">Sensor</option>
                      <option value="Controller">Controller</option>
                      <option value="Gateway">Gateway</option>
                      <option value="Camera">Camera</option>
                      <option value="Other">Other</option>
                    </select>
                  </div>
                </div>
              </div>
              
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Manufacturer</label>
                    <input v-model="newDevice.manufacturer" type="text" class="form-control" />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Model</label>
                    <input v-model="newDevice.model" type="text" class="form-control" />
                  </div>
                </div>
              </div>

              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Serial Number</label>
                    <input v-model="newDevice.serialNumber" type="text" class="form-control" />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">MAC Address</label>
                    <input v-model="newDevice.macAddress" type="text" class="form-control" />
                  </div>
                </div>
              </div>

              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Location</label>
                    <input v-model="newDevice.location" type="text" class="form-control" />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Heartbeat Interval (seconds)</label>
                    <input v-model.number="newDevice.heartbeatInterval" type="number" class="form-control" min="10" max="3600" />
                  </div>
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Description</label>
                <textarea v-model="newDevice.description" class="form-control" rows="3"></textarea>
              </div>

              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Firmware Version</label>
                    <input v-model="newDevice.firmwareVersion" type="text" class="form-control" />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Hardware Version</label>
                    <input v-model="newDevice.hardwareVersion" type="text" class="form-control" />
                  </div>
                </div>
              </div>
            </form>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="showAddModal = false">Cancel</button>
            <button class="btn btn-primary" @click="addDevice" :disabled="!newDevice.deviceName || !newDevice.deviceType">
              Add Device
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Device Details Modal -->
    <div class="modal fade show" tabindex="-1" style="display: block" v-if="showDetailsModal && selectedDevice">
      <div class="modal-dialog modal-xl modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header bg-light">
            <h5 class="modal-title">Device Details - {{ selectedDevice.deviceName }}</h5>
            <button type="button" class="btn-close" @click="showDetailsModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col-md-6">
                <h6>Basic Information</h6>
                <table class="table table-sm">
                  <tr><td><strong>Device ID:</strong></td><td><code>{{ selectedDevice.deviceId }}</code></td></tr>
                  <tr><td><strong>Name:</strong></td><td>{{ selectedDevice.deviceName }}</td></tr>
                  <tr><td><strong>Type:</strong></td><td>{{ selectedDevice.deviceType }}</td></tr>
                  <tr><td><strong>Status:</strong></td><td>
                    <span class="badge" :class="getStatusBadgeClass(selectedDevice.status)">
                      {{ selectedDevice.status }}
                    </span>
                  </td></tr>
                  <tr><td><strong>Location:</strong></td><td>{{ selectedDevice.location || 'N/A' }}</td></tr>
                  <tr><td><strong>IP Address:</strong></td><td><code>{{ selectedDevice.lastKnownIP || 'N/A' }}</code></td></tr>
                </table>
              </div>
              <div class="col-md-6">
                <h6>Technical Information</h6>
                <table class="table table-sm">
                  <tr><td><strong>Manufacturer:</strong></td><td>{{ selectedDevice.manufacturer || 'N/A' }}</td></tr>
                  <tr><td><strong>Model:</strong></td><td>{{ selectedDevice.model || 'N/A' }}</td></tr>
                  <tr><td><strong>Serial Number:</strong></td><td>{{ selectedDevice.serialNumber || 'N/A' }}</td></tr>
                  <tr><td><strong>MAC Address:</strong></td><td>{{ selectedDevice.macAddress || 'N/A' }}</td></tr>
                  <tr><td><strong>Firmware:</strong></td><td>{{ selectedDevice.firmwareVersion || 'N/A' }}</td></tr>
                  <tr><td><strong>Hardware:</strong></td><td>{{ selectedDevice.hardwareVersion || 'N/A' }}</td></tr>
                </table>
              </div>
            </div>

            <div class="row mt-3">
              <div class="col-md-4">
                <h6>Health Metrics</h6>
                <table class="table table-sm">
                  <tr><td><strong>Battery:</strong></td><td>{{ selectedDevice.batteryLevel ? selectedDevice.batteryLevel + '%' : 'N/A' }}</td></tr>
                  <tr><td><strong>Temperature:</strong></td><td>{{ selectedDevice.temperature ? selectedDevice.temperature + '°C' : 'N/A' }}</td></tr>
                  <tr><td><strong>Signal:</strong></td><td>{{ selectedDevice.signalStrength ? selectedDevice.signalStrength + ' dBm' : 'N/A' }}</td></tr>
                </table>
              </div>
              <div class="col-md-4">
                <h6>Timing Information</h6>
                <table class="table table-sm">
                  <tr><td><strong>Created:</strong></td><td>{{ formatDateTime(selectedDevice.createdAt) }}</td></tr>
                  <tr><td><strong>Last Seen:</strong></td><td>{{ selectedDevice.lastSeen ? formatDateTime(selectedDevice.lastSeen) : 'Never' }}</td></tr>
                  <tr><td><strong>Last Heartbeat:</strong></td><td>{{ selectedDevice.lastHeartbeat ? formatDateTime(selectedDevice.lastHeartbeat) : 'Never' }}</td></tr>
                </table>
              </div>
              <div class="col-md-4">
                <h6>Error Information</h6>
                <table class="table table-sm">
                  <tr><td><strong>Error Count:</strong></td><td>{{ selectedDevice.errorCount }}</td></tr>
                  <tr><td><strong>Last Error:</strong></td><td>{{ selectedDevice.lastError || 'None' }}</td></tr>
                  <tr><td><strong>Last Error Time:</strong></td><td>{{ selectedDevice.lastErrorTime ? formatDateTime(selectedDevice.lastErrorTime) : 'N/A' }}</td></tr>
                </table>
              </div>
            </div>

            <div class="mt-3" v-if="deviceEvents.length > 0">
              <h6>Recent Events</h6>
              <div class="table-responsive">
                <table class="table table-sm">
                  <thead>
                    <tr>
                      <th>Time</th>
                      <th>Type</th>
                      <th>Message</th>
                      <th>Severity</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="event in deviceEvents" :key="event.eventId">
                      <td>{{ formatDateTime(event.timestamp) }}</td>
                      <td>{{ event.eventType }}</td>
                      <td>{{ event.message }}</td>
                      <td>
                        <span class="badge" :class="getSeverityBadgeClass(event.severity)">
                          {{ event.severity }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="showDetailsModal = false">Close</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { deviceService, type Device, type DeviceEvent, type DeviceRegistrationRequest } from '../api/device'

// Reactive data
const devices = ref<Device[]>([])
const deviceEvents = ref<DeviceEvent[]>([])
const selectedDevice = ref<Device | null>(null)
const showAddModal = ref(false)
const showDetailsModal = ref(false)

// New device form
const newDevice = ref<DeviceRegistrationRequest>({
  deviceName: '',
  description: '',
  deviceType: '',
  location: '',
  firmwareVersion: '',
  hardwareVersion: '',
  manufacturer: '',
  model: '',
  serialNumber: '',
  macAddress: '',
  heartbeatInterval: 30
})

// Computed properties
const onlineDevicesCount = computed(() => devices.value.filter(d => d.status === 'Online').length)
const offlineDevicesCount = computed(() => devices.value.filter(d => d.status === 'Offline').length)
const totalDevicesCount = computed(() => devices.value.length)
const errorDevicesCount = computed(() => devices.value.filter(d => d.status === 'Error').length)

// Methods
const loadDevices = async () => {
  try {
    const result = await deviceService.getAll()
    devices.value = result
  } catch (error) {
    console.error('Error loading devices:', error)
    alert('Error loading devices: ' + error)
  }
}

const refreshDevices = () => {
  loadDevices()
}

const showAddDeviceModal = () => {
  newDevice.value = {
    deviceName: '',
    description: '',
    deviceType: '',
    location: '',
    firmwareVersion: '',
    hardwareVersion: '',
    manufacturer: '',
    model: '',
    serialNumber: '',
    macAddress: '',
    heartbeatInterval: 30
  }
  showAddModal.value = true
}

const addDevice = async () => {
  try {
    await deviceService.register(newDevice.value)
    showAddModal.value = false
    await loadDevices()
    alert('Device added successfully!')
  } catch (error) {
    console.error('Error adding device:', error)
    alert('Error adding device: ' + error)
  }
}

const viewDeviceDetails = async (device: Device) => {
  selectedDevice.value = device
  try {
    const events = await deviceService.getEvents(device.deviceId, 20)
    deviceEvents.value = events
  } catch (error) {
    console.error('Error loading device events:', error)
    deviceEvents.value = []
  }
  showDetailsModal.value = true
}

const editDevice = (device: Device) => {
  // TODO: Implement edit functionality
  alert('Edit functionality coming soon!')
}

const deleteDevice = async (device: Device) => {
  if (confirm(`Are you sure you want to delete device "${device.deviceName}"?`)) {
    try {
      await deviceService.delete(device.deviceId)
      await loadDevices()
      alert('Device deleted successfully!')
    } catch (error) {
      console.error('Error deleting device:', error)
      alert('Error deleting device: ' + error)
    }
  }
}

const cleanupOfflineDevices = async () => {
  if (confirm('Are you sure you want to cleanup offline devices?')) {
    try {
      await deviceService.cleanup(5)
      await loadDevices()
      alert('Offline devices cleanup completed!')
    } catch (error) {
      console.error('Error cleaning up devices:', error)
      alert('Error cleaning up devices: ' + error)
    }
  }
}

const getStatusBadgeClass = (status: string) => {
  switch (status) {
    case 'Online': return 'bg-success'
    case 'Offline': return 'bg-warning'
    case 'Error': return 'bg-danger'
    case 'Maintenance': return 'bg-info'
    default: return 'bg-secondary'
  }
}

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'Online': return 'fa fa-circle'
    case 'Offline': return 'fa fa-circle-o'
    case 'Error': return 'fa fa-exclamation-triangle'
    case 'Maintenance': return 'fa fa-wrench'
    default: return 'fa fa-question-circle'
  }
}

const getSeverityBadgeClass = (severity: string) => {
  switch (severity) {
    case 'Info': return 'bg-info'
    case 'Warning': return 'bg-warning'
    case 'Error': return 'bg-danger'
    case 'Critical': return 'bg-dark'
    default: return 'bg-secondary'
  }
}

const formatDateTime = (dateString: string) => {
  return new Date(dateString).toLocaleString('vi-VN')
}

const getTimeAgo = (dateString: string) => {
  const now = new Date()
  const date = new Date(dateString)
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMins / 60)
  const diffDays = Math.floor(diffHours / 24)

  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins}m ago`
  if (diffHours < 24) return `${diffHours}h ago`
  return `${diffDays}d ago`
}

// Lifecycle
onMounted(() => {
  loadDevices()
})
</script>

<style scoped>
.device-management {
  margin: 20px 0;
}

.card {
  border: none;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

.table th {
  border-top: none;
  font-weight: 600;
  color: #495057;
}

.badge {
  font-size: 0.75em;
}

.btn-group .btn {
  margin-right: 2px;
}

.modal {
  background-color: rgba(0, 0, 0, 0.5);
}

.modal-dialog {
  max-width: 90%;
}

.table-sm td {
  padding: 0.25rem 0.5rem;
}
</style>
