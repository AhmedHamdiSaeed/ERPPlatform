import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignalRService } from '../../core/services/signalr.service';

@Component({
  selector: 'app-team-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './team-chat.component.html'
})
export class TeamChatComponent {
  signalr = inject(SignalRService);

  activeChannel = signal<string>('general');
  activeDirectUser = signal<string | null>(null);

  messageInput = '';

  currentMessages = computed(() => {
    const channel = this.activeChannel();
    if (channel) {
      return this.signalr.chatMessages().filter(m => m.channelName === channel);
    }
    return [];
  });

  selectChannel(channelName: string) {
    this.activeChannel.set(channelName);
    this.activeDirectUser.set(null);
  }

  sendMessage() {
    if (!this.messageInput.trim()) return;

    this.signalr.sendChatMessage(this.activeChannel(), this.messageInput);
    this.messageInput = '';
  }
}
