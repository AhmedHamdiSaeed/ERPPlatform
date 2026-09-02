import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import {
  ChatApiService,
  ChatConversationDto,
  ChatMessageDto,
  ChatParticipantDto,
  UserLookupDto
} from '../../core/services/api/chat-api.service';
import { ChatSignalrService } from '../../core/services/chat-signalr.service';
import { StateService } from '../../core/services/state.service';
import { ToastService } from '../../core/services/toast.service';

export interface TextSegment {
  text: string;
  isMention: boolean;
}

export interface ReactionGroup {
  emoji: string;
  count: number;
  users: string[];
  mine: boolean;
}

export interface MessageVm extends ChatMessageDto {
  segments: TextSegment[];
  grouped: ReactionGroup[];
  dayLabel: string;
  showDay: boolean;
  mentionsMe: boolean;
}

const QUICK_EMOJIS = ['👍', '❤️', '😂', '🎉', '🙏', '👀'];
const TYPING_TIMEOUT_MS = 3000;

@Component({
  selector: 'app-team-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './team-chat.component.html'
})
export class TeamChatComponent implements OnInit, OnDestroy {
  private api = inject(ChatApiService);
  private signalr = inject(ChatSignalrService);
  private state = inject(StateService);
  private toast = inject(ToastService);

  private subs: Subscription[] = [];
  private typingTimer: ReturnType<typeof setTimeout> | null = null;
  private lastTypingSent = 0;

  readonly quickEmojis = QUICK_EMOJIS;

  // ---- core state ----
  conversations = signal<ChatConversationDto[]>([]);
  activeId = signal<string | null>(null);
  messages = signal<ChatMessageDto[]>([]);
  participants = signal<ChatParticipantDto[]>([]);

  loadingConversations = signal(false);
  loadingMessages = signal(false);
  sending = signal(false);
  uploading = signal(false);

  conversationFilter = '';
  messageText = '';
  messageSearchTerm = '';
  messageSearchResults = signal<ChatMessageDto[] | null>(null);

  // ---- typing indicators: key is `${conversationId}:${userId}` ----
  private typingMap = signal<Record<string, string>>({});
  private typingTimers = new Map<string, ReturnType<typeof setTimeout>>();

  // ---- composer / interactions ----
  editingId = signal<string | null>(null);
  editText = '';
  replyTo = signal<ChatMessageDto | null>(null);
  emojiPickerFor = signal<string | null>(null);
  pendingFile = signal<File | null>(null);

  // ---- dialogs / panels ----
  showNewGroup = signal(false);
  showMembers = signal(false);
  newGroupName = '';
  newGroupDescription = '';
  memberQuery = '';
  memberResults = signal<UserLookupDto[]>([]);
  selectedMembers = signal<UserLookupDto[]>([]);

  currentUserId = computed(() => this.state.currentUser().id);
  isConnected = computed(() => this.signalr.isConnected());

  activeConversation = computed(() => {
    const id = this.activeId();
    return id ? this.conversations().find(c => c.id === id) ?? null : null;
  });

  totalUnread = computed(() => this.conversations().reduce((sum, c) => sum + c.unreadCount, 0));

  filteredConversations = computed(() => {
    const term = this.conversationFilter.trim().toLowerCase();
    if (!term) return this.conversations();
    return this.conversations().filter(c => this.conversationTitle(c).toLowerCase().includes(term));
  });

  activeTypingNames = computed(() => {
    const id = this.activeId();
    if (!id) return [] as string[];
    const prefix = `${id}:`;
    const me = this.currentUserId();
    const names: string[] = [];
    for (const [key, name] of Object.entries(this.typingMap())) {
      if (key.startsWith(prefix) && !key.endsWith(`:${me}`)) names.push(name);
    }
    return names;
  });

  messageVms = computed<MessageVm[]>(() => {
    const me = this.currentUserId();
    const roster = new Map<string, string>();
    for (const p of this.activeConversation()?.participants ?? []) {
      roster.set(p.userId, p.userName);
    }

    let lastDay = '';

    return this.messages().map(m => {
      const dayLabel = this.dayLabel(m.timestamp);
      const showDay = dayLabel !== lastDay;
      lastDay = dayLabel;

      return {
        ...m,
        segments: this.segmentText(m.text, roster),
        grouped: this.groupReactions(m, me),
        dayLabel,
        showDay,
        mentionsMe: (m.mentionedUserIds ?? []).includes(me)
      };
    });
  });

  ngOnInit(): void {
    this.loadConversations();
    this.startRealtime();
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.typingTimers.forEach(t => clearTimeout(t));
    this.typingTimers.clear();
    if (this.typingTimer) clearTimeout(this.typingTimer);

    const active = this.activeId();
    if (active) void this.signalr.leaveConversation(active);
  }

  // ================= realtime =================

  private startRealtime(): void {
    this.signalr.start()
      .then(() => {
        const active = this.activeId();
        if (active) void this.signalr.joinConversation(active);
      })
      .catch(() => {
        /* HTTP still works; realtime degrades gracefully */
      });

    this.subs.push(
      this.signalr.messageReceived$.subscribe(msg => this.onMessageReceived(msg)),
      this.signalr.messageEdited$.subscribe(msg => this.replaceMessage(msg)),
      this.signalr.messageDeleted$.subscribe(e => {
        if (e.conversationId !== this.activeId()) return;
        this.messages.update(list => list.map(m =>
          m.id === e.messageId ? { ...m, isDeletedBySender: true, text: '', attachmentName: '', attachmentUrl: '' } : m));
      }),
      this.signalr.reactionChanged$.subscribe(msg => this.replaceMessage(msg)),
      this.signalr.userTyping$.subscribe(e => this.onTyping(e)),
      this.signalr.participantsChanged$.subscribe(() => {
        this.loadConversations();
        const active = this.activeId();
        if (active) this.loadParticipants(active);
      })
    );
  }

  private onMessageReceived(msg: ChatMessageDto): void {
    if (msg.conversationId === this.activeId()) {
      this.messages.update(list => list.some(m => m.id === msg.id) ? list : [...list, msg]);
      if (!msg.isMine) void this.api.markRead(msg.conversationId);
    }

    // Refresh the sidebar entry (preview + unread badge) regardless of what is open.
    this.conversations.update(list => list.map(c => c.id === msg.conversationId
      ? {
          ...c,
          lastMessageAt: msg.timestamp,
          lastMessagePreview: msg.isDeletedBySender
            ? '(message deleted)'
            : (msg.attachmentName ? `📎 ${msg.attachmentName}` : msg.text),
          lastMessageSenderName: msg.senderName,
          unreadCount: msg.conversationId === this.activeId() || msg.isMine
            ? c.unreadCount
            : c.unreadCount + 1
        }
      : c));

    if (msg.conversationId !== this.activeId() && !msg.isMine) {
      const title = this.conversations().find(c => c.id === msg.conversationId);
      this.toast.info(`${msg.senderName}: ${msg.text.slice(0, 60)}`, title ? this.conversationTitle(title) : 'New message');
    }
  }

  private replaceMessage(msg: ChatMessageDto): void {
    if (msg.conversationId !== this.activeId()) return;
    this.messages.update(list => list.map(m => m.id === msg.id ? msg : m));
  }

  private onTyping(e: { conversationId: string; userId: string; userName: string; isTyping: boolean }): void {
    const key = `${e.conversationId}:${e.userId}`;
    const existing = this.typingTimers.get(key);
    if (existing) clearTimeout(existing);

    if (!e.isTyping) {
      this.typingTimers.delete(key);
      this.removeTyping(key);
      return;
    }

    this.typingMap.update(map => ({ ...map, [key]: e.userName }));
    this.typingTimers.set(key, setTimeout(() => {
      this.typingTimers.delete(key);
      this.removeTyping(key);
    }, TYPING_TIMEOUT_MS + 1000));
  }

  private removeTyping(key: string): void {
    this.typingMap.update(map => {
      const next = { ...map };
      delete next[key];
      return next;
    });
  }

  // ================= data =================

  async loadConversations(): Promise<void> {
    this.loadingConversations.set(true);
    try {
      const list = await this.api.getMyConversations();
      this.conversations.set(list);
    } catch (err) {
      console.error('Failed to load conversations', err);
    } finally {
      this.loadingConversations.set(false);
    }
  }

  async selectConversation(id: string): Promise<void> {
    const previous = this.activeId();
    if (previous) await this.signalr.leaveConversation(previous);

    this.activeId.set(id);
    this.messages.set([]);
    this.replyTo.set(null);
    this.editingId.set(null);
    this.emojiPickerFor.set(null);
    this.messageSearchResults.set(null);
    this.messageSearchTerm = '';
    this.showMembers.set(false);

    await this.signalr.joinConversation(id);
    await this.loadMessages(id);
    await this.loadParticipants(id);

    // Clear the badge locally, then tell the server.
    this.conversations.update(list => list.map(c => c.id === id ? { ...c, unreadCount: 0 } : c));
    try {
      await this.api.markRead(id);
    } catch { /* ignore */ }
  }

  async loadMessages(id: string): Promise<void> {
    this.loadingMessages.set(true);
    try {
      this.messages.set(await this.api.getHistory(id, 0, 50));
    } catch (err) {
      console.error('Failed to load messages', err);
      this.toast.error('Could not load this conversation.', 'Chat');
    } finally {
      this.loadingMessages.set(false);
    }
  }

  async loadParticipants(id: string): Promise<void> {
    try {
      this.participants.set(await this.api.getParticipants(id));
    } catch {
      this.participants.set([]);
    }
  }

  // ================= sending =================

  onComposerInput(): void {
    const id = this.activeId();
    if (!id) return;

    const now = Date.now();
    if (now - this.lastTypingSent > 1500) {
      this.lastTypingSent = now;
      void this.signalr.setTyping(id, true);
    }

    if (this.typingTimer) clearTimeout(this.typingTimer);
    this.typingTimer = setTimeout(() => {
      void this.signalr.setTyping(id, false);
    }, TYPING_TIMEOUT_MS);
  }

  async sendMessage(): Promise<void> {
    const id = this.activeId();
    const text = this.messageText.trim();
    if (!id || !text || this.sending()) return;

    this.sending.set(true);
    try {
      const reply = this.replyTo();
      const msg = await this.api.sendMessage(id, text, reply?.id ?? null);
      this.messages.update(list => list.some(m => m.id === msg.id) ? list : [...list, msg]);
      this.messageText = '';
      this.replyTo.set(null);
      await this.signalr.setTyping(id, false);
    } catch (err) {
      console.error('Send failed', err);
      this.toast.error('Message could not be sent.', 'Chat');
    } finally {
      this.sending.set(false);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.pendingFile.set(file);
    if (file) void this.sendAttachment();
    input.value = '';
  }

  async sendAttachment(): Promise<void> {
    const id = this.activeId();
    const file = this.pendingFile();
    if (!id || !file) return;

    this.uploading.set(true);
    try {
      const msg = await this.api.sendAttachment(id, file, this.messageText.trim());
      this.messages.update(list => [...list, msg]);
      this.messageText = '';
      this.pendingFile.set(null);
      this.toast.success(`${file.name} sent.`, 'Chat');
    } catch (err) {
      console.error('Upload failed', err);
      this.toast.error('Attachment could not be uploaded (10 MB max).', 'Chat');
      this.pendingFile.set(null);
    } finally {
      this.uploading.set(false);
    }
  }

  cancelAttachment(): void {
    this.pendingFile.set(null);
  }

  // ================= edit / delete / reply =================

  startEdit(msg: ChatMessageDto): void {
    this.editingId.set(msg.id);
    this.editText = msg.text;
    this.emojiPickerFor.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editText = '';
  }

  async saveEdit(): Promise<void> {
    const id = this.editingId();
    const text = this.editText.trim();
    if (!id || !text) return;

    try {
      const updated = await this.api.editMessage(id, text);
      this.replaceMessage(updated);
      this.cancelEdit();
    } catch (err) {
      console.error('Edit failed', err);
      this.toast.error('Message could not be edited.', 'Chat');
    }
  }

  async deleteMessage(msg: ChatMessageDto): Promise<void> {
    try {
      await this.api.deleteMessage(msg.id);
      this.messages.update(list => list.map(m =>
        m.id === msg.id ? { ...m, isDeletedBySender: true, text: '', attachmentName: '', attachmentUrl: '' } : m));
    } catch (err) {
      console.error('Delete failed', err);
      this.toast.error('Message could not be deleted.', 'Chat');
    }
  }

  setReply(msg: ChatMessageDto): void {
    this.replyTo.set(msg);
    this.emojiPickerFor.set(null);
  }

  cancelReply(): void {
    this.replyTo.set(null);
  }

  // ================= reactions =================

  toggleEmojiPicker(messageId: string): void {
    this.emojiPickerFor.update(current => current === messageId ? null : messageId);
  }

  async toggleReaction(msg: ChatMessageDto, emoji: string): Promise<void> {
    try {
      const updated = await this.api.toggleReaction(msg.id, emoji);
      this.replaceMessage(updated);
    } catch (err) {
      console.error('Reaction failed', err);
    }
    this.emojiPickerFor.set(null);
  }

  // ================= group management =================

  openNewGroup(): void {
    this.showNewGroup.set(true);
    this.newGroupName = '';
    this.newGroupDescription = '';
    this.memberQuery = '';
    this.selectedMembers.set([]);
    void this.searchUsers();
  }

  closeNewGroup(): void {
    this.showNewGroup.set(false);
  }

  async searchUsers(): Promise<void> {
    // Creating a brand new group: every user is a candidate.
    // Adding to an existing group: people already in it are excluded by the API.
    const conversationId = this.isAddMemberMode() ? this.activeId() : null;
    try {
      this.memberResults.set(await this.api.getAvailableUsers(conversationId, this.memberQuery));
    } catch {
      this.memberResults.set([]);
    }
  }

  isSelected(user: UserLookupDto): boolean {
    return this.selectedMembers().some(u => u.id === user.id);
  }

  toggleMember(user: UserLookupDto): void {
    this.selectedMembers.update(list =>
      list.some(u => u.id === user.id) ? list.filter(u => u.id !== user.id) : [...list, user]);
  }

  removeSelected(user: UserLookupDto): void {
    this.selectedMembers.update(list => list.filter(u => u.id !== user.id));
  }

  async createGroup(): Promise<void> {
    const name = this.newGroupName.trim();
    if (!name) {
      this.toast.warning('Group name is required.', 'Chat');
      return;
    }

    const memberIds = this.selectedMembers().map(u => u.id);

    try {
      const conversation = await this.api.createGroup(name, this.newGroupDescription.trim(), memberIds);
      this.closeNewGroup();
      await this.loadConversations();
      await this.selectConversation(conversation.id);
      this.toast.success(`Group "${name}" created.`, 'Chat');
    } catch (err) {
      console.error('Create group failed', err);
      this.toast.error('Group could not be created.', 'Chat');
    }
  }

  async startDirectChat(userId: string): Promise<void> {
    try {
      const conversation = await this.api.startDirect(userId);
      await this.loadConversations();
      await this.selectConversation(conversation.id);
    } catch (err) {
      console.error('Direct chat failed', err);
      this.toast.error('Could not open the conversation.', 'Chat');
    }
  }

  toggleMembersPanel(): void {
    this.showMembers.update(v => !v);
    if (this.showMembers() && this.activeId()) void this.loadParticipants(this.activeId()!);
  }

  async removeMember(participant: ChatParticipantDto): Promise<void> {
    const id = this.activeId();
    if (!id) return;
    try {
      await this.api.removeMember(id, participant.userId);
      await this.loadParticipants(id);
      this.toast.success(`${participant.displayName} removed.`, 'Chat');
    } catch {
      this.toast.error('Only group admins can remove members.', 'Chat');
    }
  }

  async leaveConversation(): Promise<void> {
    const id = this.activeId();
    if (!id) return;
    try {
      await this.api.leave(id);
      this.activeId.set(null);
      this.messages.set([]);
      this.participants.set([]);
      await this.loadConversations();
    } catch {
      this.toast.error('Could not leave the conversation.', 'Chat');
    }
  }

  async toggleMute(): Promise<void> {
    const conversation = this.activeConversation();
    if (!conversation) return;
    const next = !conversation.isMuted;
    try {
      await this.api.mute(conversation.id, next);
      this.conversations.update(list =>
        list.map(c => c.id === conversation.id ? { ...c, isMuted: next } : c));
    } catch { /* ignore */ }
  }

  // ================= search =================

  async runMessageSearch(): Promise<void> {
    const term = this.messageSearchTerm.trim();
    if (!term) {
      this.messageSearchResults.set(null);
      return;
    }
    try {
      this.messageSearchResults.set(await this.api.searchMessages(this.activeId(), term));
    } catch {
      this.messageSearchResults.set([]);
    }
  }

  clearMessageSearch(): void {
    this.messageSearchTerm = '';
    this.messageSearchResults.set(null);
  }

  // ================= helpers =================

  conversationTitle(c: ChatConversationDto): string {
    if (c.type === 1) return c.name;
    const other = c.otherParticipant;
    return other ? (other.displayName || other.userName) : c.name || 'Conversation';
  }

  conversationSubtitle(c: ChatConversationDto): string {
    if (c.type === 1) return `${c.memberCount} members`;
    const other = c.otherParticipant;
    return other ? `@${other.userName}` : '';
  }

  initials(c: ChatConversationDto): string {
    const title = this.conversationTitle(c);
    if (!title) return '?';
    return title.split(' ').filter(Boolean).slice(0, 2).map(p => p[0]?.toUpperCase() ?? '').join('');
  }

  senderInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').filter(Boolean).slice(0, 2).map(p => p[0]?.toUpperCase() ?? '').join('');
  }

  preview(c: ChatConversationDto): string {
    if (!c.lastMessagePreview) return 'No messages yet';
    return c.lastMessageSenderName
      ? `${c.lastMessageSenderName}: ${c.lastMessagePreview}`
      : c.lastMessagePreview;
  }

  formatTime(value: string): string {
    if (!value) return '';
    const d = new Date(value);
    return isNaN(d.getTime()) ? '' : d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatDay(value: string): string {
    const label = this.dayLabel(value);
    return label === 'Today' || label === 'Yesterday' ? label : new Date(value).toLocaleDateString();
  }

  dayLabel(value: string): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);
    if (d.toDateString() === today.toDateString()) return 'Today';
    if (d.toDateString() === yesterday.toDateString()) return 'Yesterday';
    return d.toDateString();
  }

  isImage(msg: ChatMessageDto): boolean {
    return !!msg.attachmentContentType && msg.attachmentContentType.startsWith('image/');
  }

  formatBytes(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  isMe(participant: ChatParticipantDto): boolean {
    return participant.userId === this.currentUserId();
  }

  /** Read receipt: has anyone other than me read past this message? */
  isReadByOthers(msg: ChatMessageDto): boolean {
    const sent = new Date(msg.timestamp).getTime();
    if (!sent) return false;
    const me = this.currentUserId();
    return this.participants().some(p =>
      p.userId !== me && !!p.lastReadAt && new Date(p.lastReadAt).getTime() >= sent);
  }

  /** The new-group dialog doubles as "add members" when a group is already open. */
  isAddMemberMode = computed(() => {
    const c = this.activeConversation();
    return !!c && c.type === 1;
  });

  confirmGroupDialog(): void {
    if (this.isAddMemberMode()) {
      void this.addMembersToActiveGroup();
    } else {
      void this.createGroup();
    }
  }

  async addMembersToActiveGroup(): Promise<void> {
    const conversation = this.activeConversation();
    if (!conversation) return;

    const ids = this.selectedMembers().map(u => u.id);
    if (ids.length === 0) {
      this.toast.warning('Select at least one user.', 'Chat');
      return;
    }

    try {
      await this.api.addMembers(conversation.id, ids);
      this.closeNewGroup();
      await this.loadParticipants(conversation.id);
      await this.loadConversations();
      this.toast.success(`${ids.length} member(s) added.`, 'Chat');
    } catch (err) {
      console.error('Add members failed', err);
      this.toast.error('Only group admins can add members.', 'Chat');
    }
  }

  private segmentText(text: string, roster: Map<string, string>): TextSegment[] {
    if (!text) return [];
    if (!text.includes('@')) return [{ text, isMention: false }];

    const known = new Set([...roster.values()].filter(Boolean).map(n => n.toLowerCase()));
    const segments: TextSegment[] = [];
    const regex = /(@[\w.\-]+)/g;
    let lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = regex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        segments.push({ text: text.slice(lastIndex, match.index), isMention: false });
      }
      segments.push({ text: match[0], isMention: known.has(match[0].slice(1).toLowerCase()) });
      lastIndex = match.index + match[0].length;
    }

    if (lastIndex < text.length) {
      segments.push({ text: text.slice(lastIndex), isMention: false });
    }
    return segments;
  }

  private groupReactions(msg: ChatMessageDto, me: string): ReactionGroup[] {
    const map = new Map<string, ReactionGroup>();
    for (const r of msg.reactions ?? []) {
      const existing = map.get(r.emoji);
      if (existing) {
        existing.count++;
        existing.users.push(r.userName);
        existing.mine = existing.mine || r.userId === me;
      } else {
        map.set(r.emoji, {
          emoji: r.emoji,
          count: 1,
          users: [r.userName],
          mine: r.userId === me
        });
      }
    }
    return [...map.values()];
  }
}
