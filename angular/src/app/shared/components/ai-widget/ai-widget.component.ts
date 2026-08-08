import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { AiService, ChatMessage } from '../../../core/services/ai.service';

@Component({
  selector: 'app-ai-widget',
  standalone: true,
  imports: [FormsModule],
  template: `
    @if (state.aiWidgetOpen()) {
      <div class="fixed inset-y-0 right-0 z-50 w-full sm:w-96 bg-[var(--bg-card)] border-l border-[var(--border-color)] shadow-2xl flex flex-col animate-fade-in">
        
        <!-- Header -->
        <div class="p-4 border-b border-[var(--border-color)] bg-gradient-to-r from-blue-600 to-indigo-600 text-white flex items-center justify-between">
          <div class="flex items-center gap-2.5">
            <div class="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center text-sm backdrop-blur-xs">
              <i class="pi pi-sparkles"></i>
            </div>
            <div>
              <h3 class="font-bold text-sm leading-tight">ERP AI Assistant</h3>
              <p class="text-[10px] text-blue-100 opacity-90">Powered by Enterprise Intelligence</p>
            </div>
          </div>
          <button (click)="state.toggleAiWidget(false)" class="text-white/80 hover:text-white p-1 rounded-md">
            <i class="pi pi-times text-lg"></i>
          </button>
        </div>

        <!-- Messages Body -->
        <div class="p-4 overflow-y-auto space-y-4 flex-1 bg-slate-50/50 dark:bg-slate-900/30">
          @for (msg of messages(); track msg.id) {
            <div [class.text-right]="msg.sender === 'user'">
              <div 
                [class]="msg.sender === 'user' 
                  ? 'bg-blue-600 text-white ml-auto rounded-2xl rounded-br-none' 
                  : 'bg-white dark:bg-slate-800 border border-[var(--border-color)] text-[var(--text-main)] rounded-2xl rounded-bl-none shadow-xs'"
                class="inline-block max-w-[85%] p-3 text-xs text-left">
                <p class="whitespace-pre-line">{{ msg.text }}</p>

                <!-- AI Generated Workflow Preview Card -->
                @if (msg.generatedWorkflow) {
                  <div class="mt-3 p-3 bg-indigo-50 dark:bg-slate-700/60 rounded-xl border border-indigo-200 dark:border-slate-600 space-y-2">
                    <div class="flex items-center justify-between">
                      <span class="font-bold text-indigo-900 dark:text-indigo-200 text-xs">{{ msg.generatedWorkflow.name }}</span>
                      <span class="text-[9px] px-1.5 py-0.5 bg-indigo-200 dark:bg-indigo-900 text-indigo-800 dark:text-indigo-200 font-bold rounded">{{ msg.generatedWorkflow.nodes.length }} Nodes</span>
                    </div>
                    <p class="text-[11px] text-indigo-700 dark:text-indigo-300">{{ msg.generatedWorkflow.description }}</p>
                    <div class="flex items-center gap-2 pt-1">
                      <button (click)="openGeneratedWorkflow()" class="flex-1 py-1.5 px-2 bg-indigo-600 text-white rounded-md font-semibold text-[11px] hover:bg-indigo-700 transition-colors">
                        Open in Designer
                      </button>
                    </div>
                  </div>
                }

                <span class="text-[9px] opacity-60 block mt-1 text-right">{{ msg.timestamp }}</span>
              </div>
            </div>
          }

          @if (loading()) {
            <div class="flex items-center gap-2 text-xs text-[var(--text-muted)] bg-white dark:bg-slate-800 p-2.5 rounded-xl border border-[var(--border-color)] w-max">
              <i class="pi pi-spin pi-spinner text-blue-600"></i> AI is thinking...
            </div>
          }
        </div>

        <!-- Suggested Prompts -->
        <div class="px-3 py-2 border-t border-[var(--border-color)] bg-slate-100/50 dark:bg-slate-900/50 flex gap-1.5 overflow-x-auto text-[11px]">
          <button (click)="sendSuggested('Show employees who joined this month')" class="px-2.5 py-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-full shrink-0 hover:border-blue-500 text-[var(--text-muted)]">
            👥 Joined employees
          </button>
          <button (click)="sendSuggested('When leave > 5 days send to HR')" class="px-2.5 py-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-full shrink-0 hover:border-blue-500 text-[var(--text-muted)]">
            ⚡ Generate Workflow
          </button>
          <button (click)="sendSuggested('Explain inventory stock level changes')" class="px-2.5 py-1 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-full shrink-0 hover:border-blue-500 text-[var(--text-muted)]">
            📦 Stock changes
          </button>
        </div>

        <!-- Input Bar -->
        <div class="p-3 border-t border-[var(--border-color)] bg-[var(--bg-card)] flex items-center gap-2">
          <input 
            type="text" 
            [(ngModel)]="inputText" 
            (keyup.enter)="send()"
            placeholder="Ask AI anything or describe a workflow..."
            class="flex-1 px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-lg text-xs text-[var(--text-main)] focus:outline-hidden focus:border-blue-500"
          />
          <button 
            (click)="send()" 
            [disabled]="!inputText.trim() || loading()"
            class="p-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors">
            <i class="pi pi-send text-xs"></i>
          </button>
        </div>

      </div>
    }
  `
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
