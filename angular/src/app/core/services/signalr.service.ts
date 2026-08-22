import { Injectable, signal, inject } from '@angular/core';
import { ToastService } from './toast.service';
import { NotificationItem } from '../models/erp-models';
import { MOCK_NOTIFICATIONS } from '../mock/mock-data';

export interface ChatMessageItem {
  id: string;
  senderId: string;
  senderName: string;
  senderAvatar: string;
  channelName?: string;
  receiverId?: string;
  text: string;
  timestamp: string;
  isRead: boolean;
}

export interface ChannelInfo {
  name: string;
  label: string;
  icon: string;
  unreadCount: number;
}

@ComponentChatMockData
export const INITIAL_CHAT_MESSAGES: ChatMessageItem[] = [
  {
    id: 'msg-1',
    senderId: 'emp-2',
    senderName: 'Sara Mahmoud',
    senderAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&auto=format&fit=crop&q=80',
    channelName: 'general',
    text: 'Good morning team! Quarterly review meeting starts at 11:00 AM.',
    timestamp: '10:15 AM',
    isRead: true
  },
  {
    id: 'msg-2',
    senderId: 'emp-3',
    senderName: 'Omar Farouk',
    senderAvatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80',
    channelName: 'general',
    text: 'Thanks Sara! I have prepared the inventory stock valuation slides.',
    timestamp: '10:18 AM',
    isRead: true
  },
  {
    id: 'msg-3',
    senderId: 'usr-001',
    senderName: 'Ahmed Hamdi',
    senderAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
    channelName: 'general',
    text: 'Great! Also, new SignalR real-time hubs have been enabled for real-time notifications.',
    timestamp: '10:25 AM',
    isRead: true
  }
];

function ComponentChatMockData(target: any) {}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private toast = inject(ToastService);

  // Connection status signal
  isConnected = signal<boolean>(true);

  // Messages signal
  chatMessages = signal<ChatMessageItem[]>(INITIAL_CHAT_MESSAGES);

  // Real-time notifications signal
  notifications = signal<NotificationItem[]>(MOCK_NOTIFICATIONS);

  // Online active users
  onlineUsers = signal<{ id: string; name: string; avatar: string; status: 'online' | 'offline' }[]>([
    { id: 'emp-2', name: 'Sara Mahmoud', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&auto=format&fit=crop&q=80', status: 'online' },
    { id: 'emp-3', name: 'Omar Farouk', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80', status: 'online' },
    { id: 'emp-4', name: 'Mona Zaki', avatar: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80', status: 'offline' }
  ]);

  // Channels
  channels = signal<ChannelInfo[]>([
    { name: 'general', label: 'General Announcement', icon: 'pi-hashtag', unreadCount: 0 },
    { name: 'engineering', label: 'Engineering & Dev', icon: 'pi-code', unreadCount: 2 },
    { name: 'hr-team', label: 'HR & Recruitment', icon: 'pi-users', unreadCount: 0 },
    { name: 'inventory-alerts', label: 'Stock & Logistics Alerts', icon: 'pi-box', unreadCount: 1 }
  ]);

  sendChatMessage(channelName: string, text: string, receiverId?: string) {
    if (!text.trim()) return;

    const newMsg: ChatMessageItem = {
      id: `msg-${Date.now()}`,
      senderId: 'usr-001',
      senderName: 'Ahmed Hamdi',
      senderAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
      channelName: channelName,
      receiverId: receiverId,
      text: text.trim(),
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isRead: true
    };

    this.chatMessages.update(list => [...list, newMsg]);

    // Simulate real-time automated bot/colleague response after 2 seconds
    if (channelName === 'general') {
      setTimeout(() => {
        this.receiveSimulatedMessage('Sara Mahmoud', 'Got it Ahmed! Real-time SignalR hubs are working smoothly.', 'general');
      }, 1800);
    }
  }

  receiveSimulatedMessage(senderName: string, text: string, channelName: string) {
    const incoming: ChatMessageItem = {
      id: `msg-${Date.now()}`,
      senderId: 'emp-2',
      senderName: senderName,
      senderAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&auto=format&fit=crop&q=80',
      channelName: channelName,
      text: text,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isRead: false
    };

    this.chatMessages.update(list => [...list, incoming]);
    this.toast.info(`New message from ${senderName} in #${channelName}`, 'SignalR Chat');
  }

  broadcastRealTimeNotification(type: 'Workflow Approval' | 'System' | 'HR' | 'Inventory' | 'AI', title: string, message: string, link?: string) {
    const notif: NotificationItem = {
      id: `notif-${Date.now()}`,
      type: type,
      title: title,
      message: message,
      timestamp: 'Just now (SignalR Live)',
      read: false,
      link: link || '/notifications'
    };

    this.notifications.update(list => [notif, ...list]);
    this.toast.warning(`${title}: ${message}`, `SignalR Real-Time Alert`);
  }
}
