import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { AiService, ChatMessage } from '../../../core/services/ai.service';

@Component({
  selector: 'app-ai-widget',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './ai-widget.component.html'
})
export class AiWidgetComponent {
  state = inject(StateService);
  aiService = inject(AiService);
  router = inject(Router);

  inputText = '';
  loading = signal(false);
  messages = signal<ChatMessage[]>([
    {
      id: 'init-1',
      sender: 'ai',
      text: 'Hello Ahmed! I am your AI ERP Assistant. How can I help you analyze HR data, check stock levels, or generate custom workflows today?',
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

  sendSuggested(promptText: string) {
    this.inputText = promptText;
    this.send();
  }

  openGeneratedWorkflow() {
    this.state.toggleAiWidget(false);
    this.router.navigateByUrl('/workflow/designer');
  }
}
