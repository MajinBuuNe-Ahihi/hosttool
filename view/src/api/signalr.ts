import * as signalR from '@microsoft/signalr'

export interface Notification {
  title: string
  message: string
  type: 'info' | 'success' | 'warning' | 'error'
  timestamp: string
  from: string
  groupName?: string
}

export interface ClientConnection {
  connectionId: string
  clientIp: string
  userAgent: string
  connectedAt: string
  totalConnections: number
}

export interface ChatMessage {
  message: string
  sender: string
  timestamp: string
}

export interface WelcomeMessage {
  message: string
  connectionId: string
  connectedAt: string
}

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private isConnected = false
  private connectionId = ''

  // Event handlers
  private onNotificationCallback?: (notification: Notification) => void
  private onClientConnectedCallback?: (client: ClientConnection) => void
  private onClientDisconnectedCallback?: (client: { connectionId: string; disconnectedAt: string; totalConnections: number }) => void
  private onWelcomeCallback?: (welcome: WelcomeMessage) => void
  private onMessageCallback?: (message: ChatMessage) => void
  private onConnectedClientsCallback?: (clients: Array<{ connectionId: string; info: string }>) => void
  private onUserJoinedGroupCallback?: (data: { connectionId: string; groupName: string; joinedAt: string }) => void
  private onUserLeftGroupCallback?: (data: { connectionId: string; groupName: string; leftAt: string }) => void

  async startConnection(): Promise<void> {
    if (this.connection) {
      return
    }
debugger
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('https://hosttool.onrender.com/notificationHub', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    // Register event handlers
    this.connection.on('ReceiveNotification', (notification: Notification) => {
      console.log('Received notification:', notification)
      this.onNotificationCallback?.(notification)
    })

    this.connection.on('ClientConnected', (client: ClientConnection) => {
      console.log('Client connected:', client)
      this.onClientConnectedCallback?.(client)
    })

    this.connection.on('ClientDisconnected', (client: { connectionId: string; disconnectedAt: string; totalConnections: number }) => {
      console.log('Client disconnected:', client)
      this.onClientDisconnectedCallback?.(client)
    })

    this.connection.on('WelcomeMessage', (welcome: WelcomeMessage) => {
      console.log('Welcome message:', welcome)
      this.connectionId = welcome.connectionId
      this.onWelcomeCallback?.(welcome)
    })

    this.connection.on('ReceiveMessage', (message: ChatMessage) => {
      console.log('Received message:', message)
      this.onMessageCallback?.(message)
    })

    this.connection.on('ConnectedClientsList', (clients: Array<{ connectionId: string; info: string }>) => {
      console.log('Connected clients:', clients)
      this.onConnectedClientsCallback?.(clients)
    })

    this.connection.on('UserJoinedGroup', (data: { connectionId: string; groupName: string; joinedAt: string }) => {
      console.log('User joined group:', data)
      this.onUserJoinedGroupCallback?.(data)
    })

    this.connection.on('UserLeftGroup', (data: { connectionId: string; groupName: string; leftAt: string }) => {
      console.log('User left group:', data)
      this.onUserLeftGroupCallback?.(data)
    })

    // Connection state handlers
    this.connection.onclose((error) => {
      console.log('Connection closed:', error)
      this.isConnected = false
    })

    this.connection.onreconnecting((error) => {
      console.log('Reconnecting:', error)
      this.isConnected = false
    })

    this.connection.onreconnected((connectionId) => {
      console.log('Reconnected:', connectionId)
      this.isConnected = true
      this.connectionId = connectionId || ''
    })

    try {
      await this.connection.start()
      this.isConnected = true
      console.log('SignalR connection started successfully')
    } catch (error) {
      console.error('Error starting SignalR connection:', error)
      throw error
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.isConnected = false
      this.connectionId = ''
      console.log('SignalR connection stopped')
    }
  }

  // Public methods to send messages
  async sendNotificationToAll(title: string, message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info'): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('SendNotificationToAll', title, message, type)
  }

  async sendMessageToAll(message: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('SendMessageToAll', message)
  }

  async joinGroup(groupName: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('JoinGroup', groupName)
  }

  async leaveGroup(groupName: string): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('LeaveGroup', groupName)
  }

  async sendNotificationToGroup(groupName: string, title: string, message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info'): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('SendNotificationToGroup', groupName, title, message, type)
  }

  async getConnectedClients(): Promise<void> {
    if (!this.connection || !this.isConnected) {
      throw new Error('SignalR connection is not established')
    }
    await this.connection.invoke('GetConnectedClients')
  }

  // Event handler setters
  setOnNotification(callback: (notification: Notification) => void): void {
    this.onNotificationCallback = callback
  }

  setOnClientConnected(callback: (client: ClientConnection) => void): void {
    this.onClientConnectedCallback = callback
  }

  setOnClientDisconnected(callback: (client: { connectionId: string; disconnectedAt: string; totalConnections: number }) => void): void {
    this.onClientDisconnectedCallback = callback
  }

  setOnWelcome(callback: (welcome: WelcomeMessage) => void): void {
    this.onWelcomeCallback = callback
  }

  setOnMessage(callback: (message: ChatMessage) => void): void {
    this.onMessageCallback = callback
  }

  setOnConnectedClients(callback: (clients: Array<{ connectionId: string; info: string }>) => void): void {
    this.onConnectedClientsCallback = callback
  }

  setOnUserJoinedGroup(callback: (data: { connectionId: string; groupName: string; joinedAt: string }) => void): void {
    this.onUserJoinedGroupCallback = callback
  }

  setOnUserLeftGroup(callback: (data: { connectionId: string; groupName: string; leftAt: string }) => void): void {
    this.onUserLeftGroupCallback = callback
  }

  // Getters
  getConnectionId(): string {
    return this.connectionId
  }

  getIsConnected(): boolean {
    return this.isConnected
  }
}

export const signalRService = new SignalRService()
