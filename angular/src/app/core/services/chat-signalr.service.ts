import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { ChatMessageDto } from './api/chat-api.service';

export interface TypingEvent {
  conversationId: string;
  userId: string;
  userName: string;
  isTyping: boolean;
}

export interface MessageDeletedEvent {
  conversationId: string;
  messageId: string;
}

export interface ConversationReadEvent {
  conversationId: string;
  userId: string;
  readAt: string;
}

export interface ParticipantsChangedEvent {
  conversationId: string;
}

/**
 * Real SignalR transport for group chat, replacing the simulated SignalRService.
 *
 * The backend fans events out over a per-user group ("u:{userId}"), so a user receives
 * updates for every conversation they belong to - not just the one on screen. That is
 * what drives unread badges for threads in the background.
 */
@Injectable({ providedIn: 'root' })
export class ChatSignalrService {
  private auth = inject(AuthService);

  private connection: signalR.HubConnection | null = null;
  private readonly joined = new Set<string>();
  private startPromise: Promise<void> | null = null;

  readonly isConnected = signal<boolean>(false);

  private readonly messageReceived = new Subject<ChatMessageDto>();
  private readonly messageEdited = new Subject<ChatMessageDto>();
  private readonly messageDeleted = new Subject<MessageDeletedEvent>();
  private readonly reactionChanged = new Subject<ChatMessageDto>();
  private readonly userTyping = new Subject<TypingEvent>();
  private readonly conversationRead = new Subject<ConversationReadEvent>();
  private readonly participantsChanged = new Subject<ParticipantsChangedEvent>();

  readonly messageReceived$ = this.messageReceived.asObservable();
  readonly messageEdited$ = this.messageEdited.asObservable();
  readonly messageDeleted$ = this.messageDeleted.asObservable();
  readonly reactionChanged$ = this.reactionChanged.asObservable();
  readonly userTyping$ = this.userTyping.asObservable();
  readonly conversationRead$ = this.conversationRead.asObservable();
  readonly participantsChanged$ = this.participantsChanged.asObservable();

  private get hubUrl(): string {
    return `${environment.apis.default.url}/signalr-hubs/chat`;
  }

  start(): Promise<void> {
    if (this.startPromise) return this.startPromise;

    this.startPromise = this.startInternal().catch(err => {
      console.error('[chat-signalr] failed to start', err);
      this.isConnected.set(false);
      this.startPromise = null;
      throw err;
    });

    return this.startPromise;
  }

  private async startInternal(): Promise<void> {
    const token = this.auth.getToken();
    if (!token) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, { accessTokenFactory: () => this.auth.getToken() })
      .withAutomaticReconnect()
      .build();

    this.connection.on('MessageReceived', (message: ChatMessageDto) => this.messageReceived.next(message));
    this.connection.on('MessageEdited', (message: ChatMessageDto) => this.messageEdited.next(message));
    this.connection.on('MessageDeleted', (e: MessageDeletedEvent) => this.messageDeleted.next(e));
    this.connection.on('MessageReactionChanged', (message: ChatMessageDto) => this.reactionChanged.next(message));
    this.connection.on('UserTyping', (c: string, userId: string, userName: string, isTyping: boolean) =>
      this.userTyping.next({ conversationId: c, userId, userName, isTyping }));
    this.connection.on('ConversationRead', (e: ConversationReadEvent) => this.conversationRead.next(e));
    this.connection.on('ParticipantsChanged', (e: ParticipantsChangedEvent) => this.participantsChanged.next(e));

    // Re-subscribe to the open threads after an automatic reconnect, otherwise the
    // server no longer knows which conversations this connection is viewing.
    this.connection.onreconnected(() => {
      this.isConnected.set(true);
      for (const conversationId of this.joined) {
        this.invoke('JoinConversation', conversationId);
      }
    });

    this.connection.onclose(() => this.isConnected.set(false));

    await this.connection.start();
    this.isConnected.set(true);
  }

  async stop(): Promise<void> {
    this.startPromise = null;
    this.joined.clear();
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
    this.isConnected.set(false);
  }

  /** Registers this connection for typing indicators in the given conversation. */
  async joinConversation(conversationId: string): Promise<void> {
    this.joined.add(conversationId);
    await this.invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    this.joined.delete(conversationId);
    await this.invoke('LeaveConversation', conversationId);
  }

  async setTyping(conversationId: string, isTyping: boolean): Promise<void> {
    await this.invoke('SetTyping', conversationId, isTyping);
  }

  private async invoke(method: string, ...args: unknown[]): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      // Not fatal - the UI keeps working from HTTP; real-time events simply resume later.
      return;
    }
    try {
      await this.connection.invoke(method, ...args);
    } catch (err) {
      console.error(`[chat-signalr] ${method} failed`, err);
    }
  }
}
