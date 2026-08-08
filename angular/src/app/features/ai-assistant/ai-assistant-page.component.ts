import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AiService, ChatMessage } from '../../core/services/ai.service';

@Component({
  selector: 'app-ai-assistant-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './ai-assistant-page.component.html'
})
export class AiAssistantPageComponent {
  aiService = inject(AiService);
  router = inject(Router);

  inputText = '';
  loading = signal(false);
  messages = signal<ChatMessage[]>([
    {
      id: 'init-1',
      sender: 'ai',
      text: 'Welcome to the AI Assistant & Workflow Studio! You can ask questions about your workforce, analyze inventory trends, or describe any approval workflow in plain language to generate visual execution nodes.',
      timestamp: 'Just now'
    }
  ]);

  send() {
    const text = this.inputText.trim();
    if (!text || this.loading()) return;

    const userMsg: ChatMessage = {
      id: `user-${Date.now()}`,
      sender: 'user',
      text: text,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    };

    this.messages.update(list => [...list, userMsg]);
    this.inputText = '';
    this.loading.set(true);

    this.aiService.askAi(text).subscribe(aiReply => {
      this.messages.update(list => [...list, aiReply]);
      this.loading.set(false);
    });
  }

  openDesigner() {
    this.router.navigateByUrl('/workflow/designer');
  }
}
