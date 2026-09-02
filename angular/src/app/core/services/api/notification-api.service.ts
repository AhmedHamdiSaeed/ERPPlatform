import { Injectable } from '@angular/core';
import { ErpApiService, AbpEntity } from './erp-api.service';
import { NotificationItem } from '../../models/erp-models';

interface NotificationDto extends AbpEntity {
  userId: string;
  type: string;
  title: string;
  message: string;
  link: string;
  timestamp: string;
  isRead: boolean;
}

export interface SendNotificationInput {
  userId?: string;
  type: NotificationItem['type'];
  title: string;
  message: string;
  link?: string;
}

/** Formats an ISO timestamp as a short relative label ("5 mins ago"). */
export function timeAgo(value: string): string {
  const then = new Date(value).getTime();
  if (isNaN(then)) return '';

  const seconds = Math.floor((Date.now() - then) / 1000);
  if (seconds < 45) return 'Just now';

  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${Math.max(minutes, 1)} min${minutes === 1 ? '' : 's'} ago`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hour${hours === 1 ? '' : 's'} ago`;

  const days = Math.floor(hours / 24);
  if (days < 7) return `${days} day${days === 1 ? '' : 's'} ago`;

  return new Date(value).toLocaleDateString();
}

@Injectable({ providedIn: 'root' })
export class NotificationApiService extends ErpApiService {
  getNotifications(): Promise<NotificationItem[]> {
    return this.getList<NotificationDto>('notification/notifications').then(items =>
      items.map(n => ({
        id: n.id,
        type: n.type as NotificationItem['type'],
        title: n.title,
        message: n.message,
        timestamp: timeAgo(n.timestamp),
        read: n.isRead,
        link: n.link
      })) as NotificationItem[]
    );
  }

  sendNotification(input: SendNotificationInput): Promise<NotificationDto> {
    return this.post<NotificationDto>('notification/send-notification', {
      userId: input.userId ?? '',
      type: input.type,
      title: input.title,
      message: input.message,
      link: input.link ?? ''
    });
  }

  markAsRead(id: string): Promise<void> {
    return this.post(`notification/${id}/mark-as-read`, {});
  }

  markAllAsRead(): Promise<void> {
    return this.post('notification/mark-all-as-read', {});
  }
}
