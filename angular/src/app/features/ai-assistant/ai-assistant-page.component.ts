import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AiService, ChatMessage } from '../../core/services/ai.service';

@Component({
  selector: 'app-ai-assistant-page',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">AI Assistant & Workflow Generator Studio</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">Interact with Enterprise AI for data analytics, workforce insights, and natural language workflow creation.</p>
        </div>
      </div>

      <div class="card-panel h-[600px] flex flex-col !p-0 overflow-hidden">
        
        <!-- Header Bar -->
        <div class="p-4 border-b border-[var(--border-color)] bg-gradient-to-r from-blue-600 to-indigo-600 text-white flex items-center justify-between">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-white/20 flex items-center justify-center text-xl backdrop-blur-xs">
              <i class="pi pi-sparkles"></i>
            </div>
            <div>
              <h3 class="font-black text-sm">ERP Intelligence Copilot</h3>
              <p class="text-xs text-blue-100 opacity-90">Natural Language Workflow Generation & Analytics Engine</p>
            </div>
          </div>
        </div>

        <!-- Chat History -->
        <div class="flex-1 p-6 overflow-y-auto space-y-4 bg-slate-50/50 dark:bg-slate-900/40">
          @for (msg of messages(); track msg.id) {
            <div [class.text-right]="msg.sender === 'user'">
              <div 
                [class]="msg.sender === 'user' 
                  ? 'bg-blue-600 text-white ml-auto rounded-2xl rounded-br-none' 
                  : 'bg-[var(--bg-card)] border border-[var(--border-color)] text-[var(--text-main)] rounded-2xl rounded-bl-none shadow-xs'"
                class="inline-block max-w-xl p-4 text-xs text-left leading-relaxed">
                
                <p class="whitespace-pre-line">{{ msg.text }}</p>

                <!-- Generated Workflow Preview Box -->
                @if (msg.generatedWorkflow) {
                  <div class="mt-4 p-4 bg-indigo-50 dark:bg-slate-800/80 rounded-2xl border border-indigo-200 dark:border-slate-700 space-y-3">
                    <div class="flex items-center justify-between">
                      <h4 class="font-extrabold text-indigo-900 dark:text-indigo-200 text-sm">{{ msg.generatedWorkflow.name }}</h4>
                      <span class="px-2 py-0.5 text-[10px] font-bold bg-indigo-200 dark:bg-indigo-900 text-indigo-800 dark:text-indigo-200 rounded-full">{{ msg.generatedWorkflow.nodes.length }} Nodes Generated</span>
                    </div>
                    <p class="text-xs text-indigo-700 dark:text-indigo-300">{{ msg.generatedWorkflow.description }}</p>
                    
                    <div class="pt-2 flex gap-2">
                      <button (click)="openDesigner()" class="btn-primary text-xs flex-1 justify-center py-2">
                        <i class="pi pi-sitemap"></i> Open in Visual Designer Studio
                      </button>
                    </div>
                  </div>
                }

                <span class="text-[10px] opacity-60 block mt-2 text-right">{{ msg.timestamp }}</span>
              </div>
            </div>
          }

          @if (loading()) {
            <div class="flex items-center gap-2 text-xs text-[var(--text-muted)] bg-[var(--bg-card)] p-3 rounded-xl border border-[var(--border-color)] w-max">
              <i class="pi pi-spin pi-spinner text-blue-600 text-sm"></i> Generating AI Intelligence response...
            </div>
          }
        </div>

        <!-- Input Bar -->
        <div class="p-4 border-t border-[var(--border-color)] bg-[var(--bg-card)] flex items-center gap-3">
          <input 
            type="text" 
            [(ngModel)]="inputText"
            (keyup.enter)="send()"
            placeholder="Type your question or request e.g. 'When leave request > 5 days send to HR'..."
            class="flex-1 px-4 py-3 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-xs text-[var(--text-main)] focus:outline-hidden"
          />
          <button (click)="send()" [disabled]="!inputText.trim() || loading()" class="btn-primary text-xs py-3 px-5">
            <i class="pi pi-send"></i> Send Prompt
          </button>
        </div>

      </div>

    </div>
  `
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
