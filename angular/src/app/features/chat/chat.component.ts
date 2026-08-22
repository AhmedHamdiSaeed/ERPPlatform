import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignalrService, ChatMessage } from '../../shared/services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex h-[calc(100vh-4rem)] bg-white dark:bg-slate-900 border-t border-slate-200 dark:border-slate-800 transition-colors duration-300">
      
      <!-- Sidebar (Contacts/Channels) -->
      <div class="w-1/4 min-w-[250px] border-r border-slate-200 dark:border-slate-800 flex flex-col">
        <div class="p-4 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between">
          <h2 class="text-xl font-semibold text-slate-800 dark:text-white">Messages</h2>
          <div class="h-8 w-8 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-600 dark:text-slate-300 cursor-pointer hover:bg-slate-200 dark:hover:bg-slate-700 transition">
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
          </div>
        </div>
        
        <div class="flex-1 overflow-y-auto p-3">
          <div class="mb-2 p-3 rounded-lg bg-indigo-50 dark:bg-indigo-900/20 cursor-pointer border border-indigo-100 dark:border-indigo-800/30">
            <div class="flex items-center space-x-3">
              <div class="relative">
                <div class="h-10 w-10 rounded-full bg-gradient-to-tr from-indigo-500 to-purple-500 flex items-center justify-center text-white font-medium">G</div>
                <span class="absolute bottom-0 right-0 h-3 w-3 rounded-full bg-emerald-500 border-2 border-white dark:border-slate-900"></span>
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold text-slate-900 dark:text-white truncate">Global Chat</p>
                <p class="text-xs text-slate-500 dark:text-slate-400 truncate">General discussions</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Chat Area -->
      <div class="flex-1 flex flex-col bg-slate-50 dark:bg-slate-900">
        <!-- Chat Header -->
        <div class="h-16 px-6 border-b border-slate-200 dark:border-slate-800 flex items-center justify-between bg-white dark:bg-slate-900">
          <div class="flex items-center space-x-3">
            <div class="h-10 w-10 rounded-full bg-gradient-to-tr from-indigo-500 to-purple-500 flex items-center justify-center text-white font-medium">G</div>
            <div>
              <h3 class="text-md font-semibold text-slate-800 dark:text-white">Global Chat</h3>
              <div class="flex items-center space-x-1">
                <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
                <span class="text-xs text-slate-500 dark:text-slate-400">Online</span>
              </div>
            </div>
          </div>
          <div class="flex space-x-2">
             <button class="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
               <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
             </button>
             <button class="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
               <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" /></svg>
             </button>
          </div>
        </div>

        <!-- Messages Area -->
        <div class="flex-1 overflow-y-auto p-6 space-y-4 bg-slate-50/50 dark:bg-slate-900/50">
          
          <div *ngIf="messages.length === 0" class="flex h-full items-center justify-center text-slate-400 dark:text-slate-500">
             <p>No messages yet. Say hi!</p>
          </div>

          <div *ngFor="let msg of messages; let i = index" class="flex flex-col max-w-lg" [ngClass]="{'self-end items-end': msg.user === currentUser, 'items-start': msg.user !== currentUser}" style="margin-left: auto; margin-right: auto;" [style.margin-left]="msg.user === currentUser ? 'auto' : '0'" [style.margin-right]="msg.user === currentUser ? '0' : 'auto'">
            <div class="text-xs text-slate-500 mb-1 ml-1" *ngIf="msg.user !== currentUser">{{msg.user}}</div>
            <div class="px-4 py-2.5 rounded-2xl shadow-sm text-sm"
                 [ngClass]="msg.user === currentUser ? 'bg-indigo-600 text-white rounded-br-none' : 'bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-200 border border-slate-100 dark:border-slate-700 rounded-bl-none'">
              {{msg.message}}
            </div>
          </div>
          
        </div>

        <!-- Input Area -->
        <div class="p-4 bg-white dark:bg-slate-900 border-t border-slate-200 dark:border-slate-800">
          <form (ngSubmit)="sendMessage()" class="flex space-x-3 items-end">
            <div class="flex-1">
              <input type="text" [(ngModel)]="newMessage" name="newMessage"
                class="block w-full rounded-full border-slate-300 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 px-4 py-3 text-sm text-slate-900 dark:text-white shadow-sm focus:border-indigo-500 focus:ring-indigo-500 transition-colors"
                placeholder="Type your message..."
                autocomplete="off">
            </div>
            <button type="submit" [disabled]="!newMessage.trim()" class="inline-flex h-11 w-11 items-center justify-center rounded-full border border-transparent bg-indigo-600 text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition">
              <svg class="h-5 w-5 ml-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
              </svg>
            </button>
          </form>
        </div>
      </div>
      
    </div>
  `
})
export class ChatComponent implements OnInit, OnDestroy {
  private signalrService = inject(SignalrService);
  private sub?: Subscription;

  messages: ChatMessage[] = [];
  newMessage: string = '';
  
  // For demo, we just assign random username or get from context if available
  currentUser = 'Me'; 

  ngOnInit() {
    this.signalrService.startConnections();
    this.sub = this.signalrService.chatMessages$.subscribe(msg => {
      this.messages.push(msg);
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  async sendMessage() {
    if (!this.newMessage.trim()) return;
    
    // We optimistically add our own message to the UI or let the broadcast handle it.
    // For SignalR broadcast, everyone gets it. We'll let the broadcast handle it so it syncs,
    // but the hub uses CurrentUser.UserName which might be null.
    // Let's just send it.
    await this.signalrService.sendChatMessage('', this.newMessage);
    this.newMessage = '';
  }
}
