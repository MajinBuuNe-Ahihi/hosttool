<template>
  <div class="notification-center">
    <!-- Connection Status -->
    <div class="connection-status mb-3">
      <div class="d-flex align-items-center">
        <div 
          class="status-indicator me-2" 
          :class="{ 'connected': isConnected, 'disconnected': !isConnected }"
        ></div>
        <span class="status-text">
          {{ isConnected ? 'Đã kết nối' : 'Mất kết nối' }}
        </span>
        <span v-if="connectionId" class="ms-2 text-muted small">
          (ID: {{ connectionId.substring(0, 8) }}...)
        </span>
      </div>
    </div>

    <!-- Notification Controls -->
    <div class="notification-controls mb-3">
      <div class="row g-2">
        <div class="col-md-6">
          <input 
            v-model="notificationTitle" 
            type="text" 
            class="form-control form-control-sm" 
            placeholder="Tiêu đề thông báo"
          >
        </div>
        <div class="col-md-6">
          <input 
            v-model="notificationMessage" 
            type="text" 
            class="form-control form-control-sm" 
            placeholder="Nội dung thông báo"
          >
        </div>
        <div class="col-md-4">
          <select v-model="notificationType" class="form-select form-select-sm">
            <option value="info">Thông tin</option>
            <option value="success">Thành công</option>
            <option value="warning">Cảnh báo</option>
            <option value="error">Lỗi</option>
          </select>
        </div>
        <div class="col-md-4">
          <button 
            @click="sendNotification" 
            class="btn btn-primary btn-sm w-100"
            :disabled="!isConnected || !notificationTitle || !notificationMessage"
          >
            Gửi thông báo
          </button>
        </div>
        <div class="col-md-4">
          <button 
            @click="getConnectedClients" 
            class="btn btn-info btn-sm w-100"
            :disabled="!isConnected"
          >
            Xem client đang kết nối
          </button>
        </div>
      </div>
    </div>

    <!-- Chat Controls -->
    <div class="chat-controls mb-3">
      <div class="row g-2">
        <div class="col-md-8">
          <input 
            v-model="chatMessage" 
            type="text" 
            class="form-control form-control-sm" 
            placeholder="Nhập tin nhắn..."
            @keyup.enter="sendMessage"
          >
        </div>
        <div class="col-md-4">
          <button 
            @click="sendMessage" 
            class="btn btn-success btn-sm w-100"
            :disabled="!isConnected || !chatMessage"
          >
            Gửi tin nhắn
          </button>
        </div>
      </div>
    </div>

    <!-- Group Controls -->
    <div class="group-controls mb-3">
      <div class="row g-2">
        <div class="col-md-6">
          <input 
            v-model="groupName" 
            type="text" 
            class="form-control form-control-sm" 
            placeholder="Tên group"
          >
        </div>
        <div class="col-md-3">
          <button 
            @click="joinGroup" 
            class="btn btn-warning btn-sm w-100"
            :disabled="!isConnected || !groupName"
          >
            Tham gia group
          </button>
        </div>
        <div class="col-md-3">
          <button 
            @click="leaveGroup" 
            class="btn btn-secondary btn-sm w-100"
            :disabled="!isConnected || !groupName"
          >
            Rời group
          </button>
        </div>
      </div>
    </div>

    <!-- Notifications List -->
    <div class="notifications-list">
      <h6 class="mb-2">Thông báo gần đây:</h6>
      <div class="notifications-container" style="max-height: 300px; overflow-y: auto;">
        <div 
          v-for="(notification, index) in notifications" 
          :key="index"
          class="notification-item alert mb-2"
          :class="`alert-${getAlertClass(notification.type)}`"
        >
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <strong>{{ notification.title }}</strong>
              <p class="mb-1">{{ notification.message }}</p>
              <small class="text-muted">
                {{ formatTime(notification.timestamp) }} - {{ notification.from }}
              </small>
            </div>
            <button 
              @click="removeNotification(index)"
              class="btn-close btn-close-sm"
              aria-label="Close"
            ></button>
          </div>
        </div>
        <div v-if="notifications.length === 0" class="text-muted text-center py-3">
          Chưa có thông báo nào
        </div>
      </div>
    </div>

    <!-- Chat Messages -->
    <div class="chat-messages mt-3" v-if="chatMessages.length > 0">
      <h6 class="mb-2">Tin nhắn chat:</h6>
      <div class="chat-container" style="max-height: 200px; overflow-y: auto;">
        <div 
          v-for="(message, index) in chatMessages" 
          :key="index"
          class="chat-item border rounded p-2 mb-2"
        >
          <div class="d-flex justify-content-between">
            <span class="fw-bold">{{ message.sender.substring(0, 8) }}...</span>
            <small class="text-muted">{{ formatTime(message.timestamp) }}</small>
          </div>
          <div>{{ message.message }}</div>
        </div>
      </div>
    </div>

    <!-- Connected Clients -->
    <div class="connected-clients mt-3" v-if="connectedClients.length > 0">
      <h6 class="mb-2">Client đang kết nối ({{ connectedClients.length }}):</h6>
      <div class="clients-container" style="max-height: 150px; overflow-y: auto;">
        <div 
          v-for="(client, index) in connectedClients" 
          :key="index"
          class="client-item border rounded p-2 mb-1"
        >
          <div class="d-flex justify-content-between">
            <span class="fw-bold">{{ client.connectionId.substring(0, 8) }}...</span>
            <small class="text-muted">{{ client.info }}</small>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { signalRService, type Notification, type ChatMessage, type ClientConnection } from '../api/signalr'

// Explicit component name for better IDE support
defineOptions({
  name: 'NotificationCenter'
})

// Reactive data
const isConnected = ref(false)
const connectionId = ref('')
const notifications = ref<Notification[]>([])
const chatMessages = ref<ChatMessage[]>([])
const connectedClients = ref<Array<{ connectionId: string; info: string }>>([])

// Form data
const notificationTitle = ref('')
const notificationMessage = ref('')
const notificationType = ref<'info' | 'success' | 'warning' | 'error'>('info')
const chatMessage = ref('')
const groupName = ref('')

// Methods
const sendNotification = async () => {
  if (!notificationTitle.value || !notificationMessage.value) return
  
  try {
    await signalRService.sendNotificationToAll(
      notificationTitle.value, 
      notificationMessage.value, 
      notificationType.value
    )
    notificationTitle.value = ''
    notificationMessage.value = ''
  } catch (error) {
    console.error('Error sending notification:', error)
    alert('Lỗi khi gửi thông báo: ' + error)
  }
}

const sendMessage = async () => {
  if (!chatMessage.value) return
  
  try {
    await signalRService.sendMessageToAll(chatMessage.value)
    chatMessage.value = ''
  } catch (error) {
    console.error('Error sending message:', error)
    alert('Lỗi khi gửi tin nhắn: ' + error)
  }
}

const joinGroup = async () => {
  if (!groupName.value) return
  
  try {
    await signalRService.joinGroup(groupName.value)
    alert(`Đã tham gia group: ${groupName.value}`)
  } catch (error) {
    console.error('Error joining group:', error)
    alert('Lỗi khi tham gia group: ' + error)
  }
}

const leaveGroup = async () => {
  if (!groupName.value) return
  
  try {
    await signalRService.leaveGroup(groupName.value)
    alert(`Đã rời group: ${groupName.value}`)
  } catch (error) {
    console.error('Error leaving group:', error)
    alert('Lỗi khi rời group: ' + error)
  }
}

const getConnectedClients = async () => {
  try {
    await signalRService.getConnectedClients()
  } catch (error) {
    console.error('Error getting connected clients:', error)
    alert('Lỗi khi lấy danh sách client: ' + error)
  }
}

const removeNotification = (index: number) => {
  notifications.value.splice(index, 1)
}

const getAlertClass = (type: string) => {
  switch (type) {
    case 'success': return 'success'
    case 'warning': return 'warning'
    case 'error': return 'danger'
    default: return 'info'
  }
}

const formatTime = (timestamp: string) => {
  return new Date(timestamp).toLocaleTimeString('vi-VN')
}

// Event handlers
const handleNotification = (notification: Notification) => {
  notifications.value.unshift(notification)
  // Giới hạn số lượng thông báo hiển thị
  if (notifications.value.length > 10) {
    notifications.value = notifications.value.slice(0, 10)
  }
}

const handleClientConnected = (client: ClientConnection) => {
  // Có thể thêm logic xử lý khi có client mới kết nối
  console.log('New client connected:', client)
}

const handleClientDisconnected = (client: { connectionId: string; disconnectedAt: string; totalConnections: number }) => {
  // Có thể thêm logic xử lý khi có client ngắt kết nối
  console.log('Client disconnected:', client)
}

const handleWelcome = (welcome: { message: string; connectionId: string; connectedAt: string }) => {
  debugger
  connectionId.value = welcome.connectionId
  isConnected.value = true
  console.log('Welcome message:', welcome)
}

const handleMessage = (message: ChatMessage) => {
  chatMessages.value.unshift(message)
  // Giới hạn số lượng tin nhắn hiển thị
  if (chatMessages.value.length > 20) {
    chatMessages.value = chatMessages.value.slice(0, 20)
  }
}

const handleConnectedClients = (clients: Array<{ connectionId: string; info: string }>) => {
  connectedClients.value = clients
}

// Lifecycle
onMounted(async () => {
  try {
    // Set up event handlers
    signalRService.setOnNotification(handleNotification)
    signalRService.setOnClientConnected(handleClientConnected)
    signalRService.setOnClientDisconnected(handleClientDisconnected)
    signalRService.setOnWelcome(handleWelcome)
    signalRService.setOnMessage(handleMessage)
    signalRService.setOnConnectedClients(handleConnectedClients)

    // Start connection
    await signalRService.startConnection()
  } catch (error) {
    console.error('Error starting SignalR connection:', error)
    alert('Lỗi khi kết nối SignalR: ' + error)
  }
})

onUnmounted(async () => {
  try {
    await signalRService.stopConnection()
  } catch (error) {
    console.error('Error stopping SignalR connection:', error)
  }
})
</script>

<style scoped>
.notification-center {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  margin: 20px 0;
}

.status-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
}

.status-indicator.connected {
  background-color: #28a745;
}

.status-indicator.disconnected {
  background-color: #dc3545;
}

.notification-item {
  border-left: 4px solid;
}

.notification-item.alert-info {
  border-left-color: #17a2b8;
}

.notification-item.alert-success {
  border-left-color: #28a745;
}

.notification-item.alert-warning {
  border-left-color: #ffc107;
}

.notification-item.alert-danger {
  border-left-color: #dc3545;
}

.chat-item {
  background: white;
}

.client-item {
  background: white;
}
</style>
