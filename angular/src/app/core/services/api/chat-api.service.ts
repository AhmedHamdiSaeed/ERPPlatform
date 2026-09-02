import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ErpApiService } from './erp-api.service';

export type ChatConversationType = 0 | 1 | 2; // 0 = Direct, 1 = Group, 2 = Channel

export interface ChatParticipantDto {
  id: string;
  conversationId: string;
  userId: string;
  userName: string;
  displayName: string;
  email: string;
  userAvatar: string;
  isAdmin: boolean;
  joinedAt: string;
  lastReadAt: string | null;
  isMuted: boolean;
}

export interface ChatConversationDto {
  id: string;
  name: string;
  description: string;
  avatarUrl: string;
  type: ChatConversationType;
  creatorUserId: string;
  creationTime: string;
  lastMessageAt: string | null;
  lastMessageSenderName: string;
  lastMessagePreview: string;
  isArchived: boolean;
  memberCount: number;
  unreadCount: number;
  isMuted: boolean;
  isAdmin: boolean;
  participants: ChatParticipantDto[];
  otherParticipant: ChatParticipantDto | null;
}

export interface ChatReactionDto {
  userId: string;
  userName: string;
  emoji: string;
}

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar: string;
  text: string;
  timestamp: string;
  isEdited: boolean;
  editedAt: string | null;
  isDeletedBySender: boolean;
  replyToMessageId: string | null;
  replyToSenderName: string;
  replyToPreview: string;
  mentionedUserIds: string[];
  attachmentName: string;
  attachmentContentType: string;
  attachmentSizeBytes: number;
  attachmentUrl: string;
  reactions: ChatReactionDto[];
  isMine: boolean;
}

export interface UserLookupDto {
  id: string;
  userName: string;
  displayName: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class ChatApiService extends ErpApiService {
  private readonly httpClient = inject(HttpClient);

  // ---------------- conversations ----------------

  getMyConversations(): Promise<ChatConversationDto[]> {
    return this.getList<ChatConversationDto>('chat-conversation/my-conversations');
  }

  createGroup(name: string, description: string, memberUserIds: string[]): Promise<ChatConversationDto> {
    return this.post<ChatConversationDto>('chat-conversation/group', { name, description, memberUserIds });
  }

  updateGroup(id: string, name: string, description: string, avatarUrl: string): Promise<ChatConversationDto> {
    return this.put<ChatConversationDto>(`chat-conversation/${id}/group`, { name, description, avatarUrl });
  }

  addMembers(id: string, memberUserIds: string[]): Promise<ChatConversationDto> {
    return this.post<ChatConversationDto>(`chat-conversation/${id}/members`, { memberUserIds });
  }

  removeMember(id: string, userId: string): Promise<void> {
    return this.delete(`chat-conversation/${id}/member/${encodeURIComponent(userId)}`);
  }

  leave(id: string): Promise<void> {
    return this.post<void>(`chat-conversation/${id}/leave`, null);
  }

  getParticipants(id: string): Promise<ChatParticipantDto[]> {
    return this.getList<ChatParticipantDto>(`chat-conversation/${id}/participants`);
  }

  startDirect(otherUserId: string): Promise<ChatConversationDto> {
    return this.post<ChatConversationDto>(`chat-conversation/start-direct/${encodeURIComponent(otherUserId)}`, null);
  }

  /**
   * User picker. When conversationId is set we scope to an existing group (the API
   * excludes current members); when null we are building a brand-new group, so we hit
   * the dedicated endpoint that only excludes the caller.
   */
  getAvailableUsers(conversationId: string | null, filter: string, maxResultCount = 20): Promise<UserLookupDto[]> {
    const q = `filter=${encodeURIComponent(filter)}&maxResultCount=${maxResultCount}`;
    if (conversationId) {
      return this.getList<UserLookupDto>(`chat-conversation/available-users/${encodeURIComponent(conversationId)}?${q}`);
    }
    return this.getList<UserLookupDto>(`chat-conversation/available-users-for-new-group?${q}`);
  }

  mute(id: string, isMuted: boolean): Promise<void> {
    return this.post<void>(`chat-conversation/${id}/mute?isMuted=${isMuted}`, null);
  }

  // ---------------- messages ----------------

  getHistory(conversationId: string, skipCount = 0, maxResultCount = 50): Promise<ChatMessageDto[]> {
    return this.getList<ChatMessageDto>(
      `chat-message/history/${encodeURIComponent(conversationId)}?skipCount=${skipCount}&maxResultCount=${maxResultCount}`
    );
  }

  sendMessage(conversationId: string, text: string, replyToMessageId?: string | null): Promise<ChatMessageDto> {
    return this.post<ChatMessageDto>('chat-message/send-message', {
      conversationId,
      text,
      replyToMessageId: replyToMessageId ?? null
    });
  }

  // NOTE: ABP maps EditMessageAsync -> POST (it only treats Put/Update as PUT), so this is a POST, not a PUT.
  editMessage(id: string, text: string): Promise<ChatMessageDto> {
    return this.post<ChatMessageDto>(`chat-message/${id}/edit-message`, { text });
  }

  deleteMessage(id: string): Promise<void> {
    return this.delete(`chat-message/${id}/message`);
  }

  markRead(conversationId: string): Promise<void> {
    return this.post<void>(`chat-message/mark-read/${encodeURIComponent(conversationId)}`, null);
  }

  toggleReaction(messageId: string, emoji: string): Promise<ChatMessageDto> {
    return this.post<ChatMessageDto>(
      `chat-message/toggle-reaction/${encodeURIComponent(messageId)}?emoji=${encodeURIComponent(emoji)}`,
      null
    );
  }

  searchMessages(conversationId: string, keyword: string, maxResultCount = 50): Promise<ChatMessageDto[]> {
    const route =
      `chat-message/search/${encodeURIComponent(conversationId)}` +
      `?keyword=${encodeURIComponent(keyword)}&maxResultCount=${maxResultCount}`;
    return firstValueFrom(
      this.httpClient.post<{ items: ChatMessageDto[] }>(`${this.apiRoot}/${route}`, null)
    ).then(res => res.items ?? []);
  }

  /** Multipart upload - posted directly because the base helper serialises to JSON. */
  sendAttachment(conversationId: string, file: File, text: string): Promise<ChatMessageDto> {
    const form = new FormData();
    form.append('file', file, file.name);

    const route = `chat-message/send-attachment/${encodeURIComponent(conversationId)}?text=${encodeURIComponent(text ?? '')}`;
    return firstValueFrom(
      this.httpClient.post<ChatMessageDto>(`${this.apiRoot}/${route}`, form)
    );
  }

  attachmentUrl(messageId: string): string {
    return `${this.apiRoot}/chat-message/attachment/${encodeURIComponent(messageId)}`;
  }
}
