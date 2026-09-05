import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignalrService } from '../../shared/services/signalr.service';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from 'src/app/shared/pipes/translate.pipe';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule,TranslatePipe],
  template: `
    <div class="min-h-screen bg-slate-50 dark:bg-slate-900 py-8 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
      <div class="max-w-3xl mx-auto">
        
        <!-- Header -->
        <div class="mb-8 flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-extrabold text-slate-900 dark:text-white tracking-tight">Notifications</h1>
            <p class="mt-2 text-sm text-slate-600 dark:text-slate-400">Stay updated with the latest events</p>
          </div>
          <div class="flex items-center space-x-3">
            <span class="relative flex h-3 w-3">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
              <span class="relative inline-flex rounded-full h-3 w-3 bg-emerald-500"></span>
            </span>
            <span class="text-sm font-medium text-emerald-600 dark:text-emerald-400">Live</span>
          </div>
        </div>

        <!-- Notification List -->
        <div class="bg-white dark:bg-slate-800 shadow-xl shadow-slate-200/50 dark:shadow-none rounded-2xl overflow-hidden border border-slate-100 dark:border-slate-700 transition-all duration-300">
          
          <!-- Empty State -->
          <div *ngIf="notifications.length === 0" class="p-12 text-center">
            <div class="mx-auto h-24 w-24 text-slate-300 dark:text-slate-600 mb-4">
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
            </div>
            <h3 class="text-lg font-medium text-slate-900 dark:text-white">{{ 'NoNotificationsYet' | translate }}</h3>
            <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">{{ 'NoNotificationsYetDescription' | translate }}</p>
          </div>

          <!-- List -->
          <ul class="divide-y divide-slate-100 dark:divide-slate-700">
            <li *ngFor="let notification of notifications; let i = index" 
                class="p-6 hover:bg-slate-50 dark:hover:bg-slate-750 transition-colors duration-200 ease-in-out group animate-fade-in-down"
                [style.animation-delay]="(i * 50) + 'ms'">
              <div class="flex items-start space-x-4">
                <div class="flex-shrink-0 mt-1">
                  <div class="h-10 w-10 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-400 group-hover:scale-110 transition-transform duration-300">
                    <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-slate-900 dark:text-white leading-5">
                    System Message
                  </p>
                  <p class="mt-1 text-sm text-slate-600 dark:text-slate-300">
                    {{ notification }}
                  </p>
                </div>
                <div class="flex-shrink-0 self-center">
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300">
                    New
                  </span>
                </div>
              </div>
            </li>
          </ul>
        </div>
        
      </div>
    </div>
  `,
  styles: [`
    @keyframes fadeInDown {
      0% { opacity: 0; transform: translateY(-10px); }
      100% { opacity: 1; transform: translateY(0); }
    }
    .animate-fade-in-down {
      animation: fadeInDown 0.4s ease-out forwards;
    }
  `]
})
export class NotificationsComponent implements OnInit, OnDestroy {
  private signalrService = inject(SignalrService);
  private sub?: Subscription;

  notifications: string[] = [];

  ngOnInit() {
    this.signalrService.startConnections();
    this.sub = this.signalrService.notifications$.subscribe(msg => {
      // Add new notification to the top
      this.notifications.unshift(msg);
    });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }
}
