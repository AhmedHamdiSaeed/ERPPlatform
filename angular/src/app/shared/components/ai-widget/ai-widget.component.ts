import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { AiService, ChatMessage } from '../../../core/services/ai.service';
import { TranslationService } from '../../../core/services/translation.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-ai-widget',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './ai-widget.component.html'
})
export class AiWidgetComponent implements OnInit {
  state = inject(StateService);
  aiService = inject(AiService);
  translation = inject(TranslationService);
  router = inject(Router);

  inputText = '';
  loading = signal(false);
  messages = signal<ChatMessage[]>([]);

  ngOnInit(): void {
    const rawUser = this.state.currentUser()?.name || this.state.currentUser()?.email || '';
    const user = rawUser.includes('@') ? rawUser.split('@')[0].replace('.', ' ') : (rawUser || 'أحمد');
    const isAr = this.state.lang() === 'ar';
    const greeting = isAr
      ? `مرحباً ${user}! أنا مساعد الذكاء الاصطناعي الخاص بك. كيف يمكنني مساعدتك اليوم في تحليل بيانات الموارد البشرية، أو فحص مستويات المخزون، أو إنشاء مسارات عمل مخصصة؟`
      : `Hello ${user}! I am your AI Assistant. How can I help you analyze HR data, check stock levels, or generate custom workflows today?`;
    
    this.messages.set([
      {
        id: 'init-1',
        sender: 'ai',
        text: greeting,
        timestamp: isAr ? 'الآن' : 'Just now'
      }
    ]);
  }

  send() {
    const text = this.inputText.trim();
    if (!text || this.loading()) return;

    const isAr = this.state.lang() === 'ar';
    const userMsg: ChatMessage = {
      id: `user-${Date.now()}`,
      sender: 'user',
      text: text,
      timestamp: new Date().toLocaleTimeString(isAr ? 'ar-EG' : 'en-US', { hour: '2-digit', minute: '2-digit' })
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
