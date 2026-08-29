import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';
import { Subject } from 'rxjs';

export interface ChatMessage {
  user: string;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private authService = inject(AuthService);

  private notificationConnection: signalR.HubConnection | null = null;
  private chatConnection: signalR.HubConnection | null = null;

  private notificationSubject = new Subject<string>();
  public notifications$ = this.notificationSubject.asObservable();

  private chatSubject = new Subject<ChatMessage>();
  public chatMessages$ = this.chatSubject.asObservable();

  private get baseUrl(): string {
    return environment.apis.default.url;
  }

  public async startConnections() {
    const token = this.authService.getToken();
    if (!token) return;

    // Initialize Notification Hub
    if (!this.notificationConnection) {
      this.notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.baseUrl}/signalr-hubs/notification`, {
          accessTokenFactory: () => this.authService.getToken()
        })
        .withAutomaticReconnect()
        .build();

      this.notificationConnection.on('ReceiveNotification', (message: string) => {
        this.notificationSubject.next(message);
      });

      try {
        await this.notificationConnection.start();
        console.log('Notification Hub connected');
      } catch (err) {
        console.error('Error connecting to Notification Hub', err);
      }
    }

    // Initialize Chat Hub
    if (!this.chatConnection) {
      this.chatConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.baseUrl}/signalr-hubs/chat`, {
          accessTokenFactory: () => this.authService.getToken()
        })
        .withAutomaticReconnect()
        .build();

      this.chatConnection.on('ReceiveMessage', (user: string, message: string) => {
        this.chatSubject.next({ user, message });
      });

      try {
        await this.chatConnection.start();
        console.log('Chat Hub connected');
      } catch (err) {
        console.error('Error connecting to Chat Hub', err);
      }
    }
  }

  public async sendChatMessage(targetUserId: string, message: string) {
    if (this.chatConnection && this.chatConnection.state === signalR.HubConnectionState.Connected) {
      await this.chatConnection.invoke('SendMessage', targetUserId, message);
    } else {
      console.error('Chat Hub is not connected');
    }
  }
}
