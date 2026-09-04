import {
  Component, OnInit, inject, signal, computed,
  ElementRef, ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  DocumentService, DocumentDto, FolderDto
} from './document.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { DialogService } from '../../core/services/dialog.service';

interface BreadcrumbItem {
  id?: string;
  name: string;
}

@Component({
  selector: 'app-document-manager',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  template: `
<div class="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-900 dark:to-slate-800">

  <!-- Header -->
  <div class="bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800 px-6 py-4 sticky top-0 z-10">
    <div class="max-w-7xl mx-auto flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-slate-900 dark:text-white">{{ 'Document Manager' | translate }}</h1>
        <p class="text-sm text-slate-500 dark:text-slate-400 mt-0.5">{{ 'Organize and manage your files' | translate }}</p>
      </div>
      <div class="flex items-center gap-3">
        <!-- Upload Button -->
        <button (click)="fileInput.click()"
          class="flex items-center gap-2 px-4 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-xl shadow-sm transition-all duration-200 hover:shadow-md active:scale-95">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/>
          </svg>
          {{ 'Upload File' | translate }}
        </button>
        <!-- New Folder Button -->
        <button (click)="showNewFolderInput = true"
          class="flex items-center gap-2 px-4 py-2.5 bg-slate-100 hover:bg-slate-200 dark:bg-slate-800 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-200 text-sm font-medium rounded-xl transition-all duration-200 active:scale-95">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m-9 1V7a2 2 0 012-2h6l2 2h6a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z"/>
          </svg>
          {{ 'New Folder' | translate }}
        </button>
        <!-- View Toggle -->
        <div class="flex items-center bg-slate-100 dark:bg-slate-800 rounded-xl p-1">
          <button (click)="viewMode = 'grid'" [class.bg-white]="viewMode==='grid'" [class.dark:bg-slate-700]="viewMode==='grid'" [class.shadow-sm]="viewMode==='grid'"
            class="p-1.5 rounded-lg transition-all">
            <svg class="h-4 w-4 text-slate-600 dark:text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"/>
            </svg>
          </button>
          <button (click)="viewMode = 'list'" [class.bg-white]="viewMode==='list'" [class.dark:bg-slate-700]="viewMode==='list'" [class.shadow-sm]="viewMode==='list'"
            class="p-1.5 rounded-lg transition-all">
            <svg class="h-4 w-4 text-slate-600 dark:text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>

  <div class="max-w-7xl mx-auto px-6 py-6">

    <!-- Breadcrumb -->
    <nav class="flex items-center gap-1 mb-6 text-sm">
      @for (crumb of breadcrumbs; track crumb.id; let last = $last; let i = $index) {
        @if (i > 0) {
          <svg class="h-4 w-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        }
        <button (click)="navigateToCrumb(i)"
          [class.text-slate-900]="last" [class.dark:text-white]="last" [class.font-semibold]="last"
          [class.text-slate-500]="!last" [class.hover:text-indigo-600]="!last"
          class="transition-colors">{{ crumb.name | translate }}</button>
      }
    </nav>

    <!-- New Folder Input -->
    @if (showNewFolderInput) {
      <div class="mb-4 flex items-center gap-3 bg-white dark:bg-slate-800 p-3 rounded-xl border border-indigo-200 dark:border-indigo-800 shadow-sm">
        <svg class="h-5 w-5 text-indigo-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m-9 1V7a2 2 0 012-2h6l2 2h6a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z"/>
        </svg>
        <input #folderInput type="text" [(ngModel)]="newFolderName" (keyup.enter)="createFolder()" (keyup.escape)="showNewFolderInput = false; newFolderName=''"
          [placeholder]="'Folder name...' | translate" autofocus
          class="flex-1 text-sm outline-none bg-transparent text-slate-900 dark:text-white placeholder-slate-400"/>
        <div class="flex gap-2">
          <button (click)="createFolder()" class="px-3 py-1.5 bg-indigo-600 text-white text-xs font-medium rounded-lg hover:bg-indigo-700 transition">{{ 'Create' | translate }}</button>
          <button (click)="showNewFolderInput = false; newFolderName=''" class="px-3 py-1.5 text-slate-500 text-xs hover:text-slate-700 dark:hover:text-slate-300 transition">{{ 'Cancel' | translate }}</button>
        </div>
      </div>
    }

    <!-- Upload Progress -->
    @if (uploading) {
      <div class="mb-4 bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-200 dark:border-indigo-800 rounded-xl p-4 flex items-center gap-3">
        <div class="h-4 w-4 border-2 border-indigo-600 border-t-transparent rounded-full animate-spin"></div>
        <span class="text-sm text-indigo-700 dark:text-indigo-300">{{ 'Uploading' | translate }} {{ uploadingFileName }}...</span>
      </div>
    }

    <!-- Drag & Drop zone (shown when folder is empty) -->
    @if (folders().length === 0 && documents().length === 0 && !loading) {
      <div (dragover)="onDragOver($event)" (dragleave)="onDragLeave($event)" (drop)="onDrop($event)"
        [class.border-indigo-400]="isDragging" [class.bg-indigo-50]="isDragging"
        class="border-2 border-dashed border-slate-300 dark:border-slate-700 rounded-2xl p-16 text-center transition-all duration-200 cursor-pointer"
        (click)="fileInput.click()">
        <div class="mx-auto h-16 w-16 bg-slate-100 dark:bg-slate-800 rounded-2xl flex items-center justify-center mb-4">
          <svg class="h-8 w-8 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"/>
          </svg>
        </div>
        <h3 class="text-lg font-semibold text-slate-700 dark:text-slate-300">{{ 'Drop files here or click to upload' | translate }}</h3>
        <p class="text-sm text-slate-500 dark:text-slate-500 mt-1">{{ 'Supports any file type up to 100 MB' | translate }}</p>
      </div>
    }

    <!-- Loading Skeleton -->
    @if (loading) {
      <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
        @for (i of [1,2,3,4,5,6]; track i) {
          <div class="bg-white dark:bg-slate-800 rounded-2xl p-4 animate-pulse">
            <div class="h-12 w-12 bg-slate-200 dark:bg-slate-700 rounded-xl mx-auto mb-3"></div>
            <div class="h-3 bg-slate-200 dark:bg-slate-700 rounded-full"></div>
          </div>
        }
      </div>
    }

    <!-- Grid View -->
    @if (!loading && viewMode === 'grid' && (folders().length > 0 || documents().length > 0)) {
      <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
        <!-- Folders -->
        @for (folder of folders(); track folder.id) {
          <div (dblclick)="openFolder(folder)"
            class="group bg-white dark:bg-slate-800 border border-slate-100 dark:border-slate-700 rounded-2xl p-4 cursor-pointer hover:border-indigo-300 dark:hover:border-indigo-600 hover:shadow-lg hover:shadow-indigo-100 dark:hover:shadow-none transition-all duration-200 relative">
            <div class="flex flex-col items-center">
              <svg class="h-12 w-12 text-amber-400 group-hover:text-amber-500 transition-colors" fill="currentColor" viewBox="0 0 24 24">
                <path d="M10 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"/>
              </svg>
              <p class="mt-3 text-xs font-medium text-slate-700 dark:text-slate-300 text-center truncate w-full">{{ folder.name }}</p>
            </div>
            <!-- Delete -->
            <button (click)="$event.stopPropagation(); deleteFolder(folder)"
              class="absolute top-2 right-2 h-6 w-6 rounded-full bg-red-500 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity text-xs hover:bg-red-600">
              ×
            </button>
          </div>
        }
        <!-- Documents -->
        @for (doc of documents(); track doc.id) {
          <div class="group bg-white dark:bg-slate-800 border border-slate-100 dark:border-slate-700 rounded-2xl p-4 hover:border-indigo-300 dark:hover:border-indigo-600 hover:shadow-lg hover:shadow-indigo-100 dark:hover:shadow-none transition-all duration-200 relative">
            <div class="flex flex-col items-center">
              <div class="h-12 w-12 rounded-xl flex items-center justify-center text-white text-xs font-bold" [style.background]="extColor(doc.extension)">
                {{ doc.extension.replace('.','').toUpperCase().slice(0,4) }}
              </div>
              <p class="mt-3 text-xs font-medium text-slate-700 dark:text-slate-300 text-center truncate w-full" [title]="doc.title + doc.extension">{{ doc.title }}</p>
              <p class="text-xs text-slate-400 mt-0.5">{{ docService.formatBytes(doc.sizeBytes) }}</p>
            </div>
            <!-- Actions -->
            <div class="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
              <button (click)="downloadDoc(doc)" class="h-6 w-6 rounded-full bg-indigo-500 text-white flex items-center justify-center hover:bg-indigo-600">
                <svg class="h-3 w-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
              </button>
              <button (click)="deleteDoc(doc)" class="h-6 w-6 rounded-full bg-red-500 text-white flex items-center justify-center hover:bg-red-600 text-xs font-bold">×</button>
            </div>
          </div>
        }
      </div>
    }

    <!-- List View -->
    @if (!loading && viewMode === 'list' && (folders().length > 0 || documents().length > 0)) {
      <div class="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden shadow-sm">
        <table class="w-full text-sm">
          <thead class="bg-slate-50 dark:bg-slate-900/50 border-b border-slate-200 dark:border-slate-700">
            <tr>
              <th class="text-left px-6 py-3 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{{ 'Name' | translate }}</th>
              <th class="text-left px-6 py-3 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{{ 'Type' | translate }}</th>
              <th class="text-left px-6 py-3 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{{ 'Size' | translate }}</th>
              <th class="text-right px-6 py-3 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">{{ 'Actions' | translate }}</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100 dark:divide-slate-700">
            @for (folder of folders(); track folder.id) {
              <tr (dblclick)="openFolder(folder)" class="hover:bg-slate-50 dark:hover:bg-slate-700/50 cursor-pointer transition-colors">
                <td class="px-6 py-4">
                  <div class="flex items-center gap-3">
                    <svg class="h-5 w-5 text-amber-400 flex-shrink-0" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M10 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"/>
                    </svg>
                    <span class="font-medium text-slate-800 dark:text-slate-200">{{ folder.name }}</span>
                  </div>
                </td>
                <td class="px-6 py-4 text-slate-500 dark:text-slate-400">{{ 'Folder' | translate }}</td>
                <td class="px-6 py-4 text-slate-500 dark:text-slate-400">—</td>
                <td class="px-6 py-4 text-right">
                  <button (click)="deleteFolder(folder)" class="text-red-400 hover:text-red-600 transition-colors text-xs font-medium">{{ 'Delete' | translate }}</button>
                </td>
              </tr>
            }
            @for (doc of documents(); track doc.id) {
              <tr class="hover:bg-slate-50 dark:hover:bg-slate-700/50 transition-colors">
                <td class="px-6 py-4">
                  <div class="flex items-center gap-3">
                    <div class="h-8 w-8 rounded-lg flex items-center justify-center text-white text-xs font-bold flex-shrink-0" [style.background]="extColor(doc.extension)">
                      {{ doc.extension.replace('.','').toUpperCase().slice(0,3) }}
                    </div>
                    <span class="font-medium text-slate-800 dark:text-slate-200">{{ doc.title }}{{ doc.extension }}</span>
                  </div>
                </td>
                <td class="px-6 py-4 text-slate-500 dark:text-slate-400">{{ doc.extension.replace('.','').toUpperCase() }}</td>
                <td class="px-6 py-4 text-slate-500 dark:text-slate-400">{{ docService.formatBytes(doc.sizeBytes) }}</td>
                <td class="px-6 py-4 text-right flex justify-end gap-3">
                  <button (click)="downloadDoc(doc)" class="text-indigo-500 hover:text-indigo-700 transition-colors text-xs font-medium">{{ 'Download' | translate }}</button>
                  <button (click)="deleteDoc(doc)" class="text-red-400 hover:text-red-600 transition-colors text-xs font-medium">{{ 'Delete' | translate }}</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  </div>

  <!-- Hidden file input -->
  <input #fileInput type="file" class="hidden" multiple (change)="onFileSelected($event)">
</div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class DocumentManagerComponent implements OnInit {
  docService = inject(DocumentService);
  private dialog = inject(DialogService);

  folders = signal<FolderDto[]>([]);
  documents = signal<DocumentDto[]>([]);
  loading = false;
  viewMode: 'grid' | 'list' = 'grid';

  breadcrumbs: BreadcrumbItem[] = [{ name: 'My Files', id: undefined }];
  currentFolderId: string | undefined = undefined;

  showNewFolderInput = false;
  newFolderName = '';

  uploading = false;
  uploadingFileName = '';
  isDragging = false;

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  ngOnInit() {
    this.loadContents();
  }

  loadContents() {
    this.loading = true;
    this.docService.getFolders(this.currentFolderId).subscribe({
      next: f => { this.folders.set(f); this.loading = false; },
      error: () => { this.folders.set([]); this.loading = false; }
    });
    this.docService.getDocuments(this.currentFolderId).subscribe({
      next: d => this.documents.set(d),
      error: () => this.documents.set([])
    });
  }

  openFolder(folder: FolderDto) {
    this.currentFolderId = folder.id;
    this.breadcrumbs.push({ name: folder.name, id: folder.id });
    this.loadContents();
  }

  navigateToCrumb(index: number) {
    this.breadcrumbs = this.breadcrumbs.slice(0, index + 1);
    this.currentFolderId = this.breadcrumbs[index].id;
    this.loadContents();
  }

  createFolder() {
    if (!this.newFolderName.trim()) return;
    this.docService.createFolder(this.newFolderName.trim(), this.currentFolderId).subscribe({
      next: folder => {
        this.folders.update(prev => [...prev, folder]);
        this.newFolderName = '';
        this.showNewFolderInput = false;
      }
    });
  }

  async deleteFolder(folder: FolderDto) {
    const confirmed = await this.dialog.confirm({
      title: 'Confirm Deletion',
      message: `Delete folder "${folder.name}"?`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      type: 'danger',
      icon: 'trash'
    });
    if (!confirmed) return;
    this.docService.deleteFolder(folder.id).subscribe({
      next: () => this.folders.update(prev => prev.filter(f => f.id !== folder.id))
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      for (const file of Array.from(input.files)) {
        this.uploadFile(file);
      }
      input.value = '';
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    const files = event.dataTransfer?.files;
    if (files) {
      for (const file of Array.from(files)) {
        this.uploadFile(file);
      }
    }
  }

  uploadFile(file: File) {
    this.uploading = true;
    this.uploadingFileName = file.name;
    this.docService.uploadFile(file, this.currentFolderId).subscribe({
      next: doc => {
        this.documents.update(prev => [...prev, doc]);
        this.uploading = false;
        this.uploadingFileName = '';
      },
      error: () => {
        this.uploading = false;
        this.uploadingFileName = '';
      }
    });
  }

  downloadDoc(doc: DocumentDto) {
    this.docService.downloadFile(doc.id).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.title + doc.extension;
        a.click();
        window.URL.revokeObjectURL(url);
      }
    });
  }

  async deleteDoc(doc: DocumentDto) {
    const confirmed = await this.dialog.confirm({
      title: 'Confirm Deletion',
      message: `Delete "${doc.title}${doc.extension}"?`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      type: 'danger',
      icon: 'trash'
    });
    if (!confirmed) return;
    this.docService.deleteDocument(doc.id).subscribe({
      next: () => this.documents.update(prev => prev.filter(d => d.id !== doc.id))
    });
  }

  extColor(ext: string): string {
    const map: Record<string, string> = {
      '.pdf': '#ef4444', '.doc': '#3b82f6', '.docx': '#3b82f6',
      '.xls': '#22c55e', '.xlsx': '#22c55e', '.ppt': '#f97316',
      '.pptx': '#f97316', '.jpg': '#8b5cf6', '.jpeg': '#8b5cf6',
      '.png': '#8b5cf6', '.gif': '#8b5cf6', '.zip': '#6b7280',
      '.rar': '#6b7280', '.txt': '#64748b', '.csv': '#22c55e',
      '.mp4': '#ec4899', '.mp3': '#ec4899',
    };
    return map[ext.toLowerCase()] ?? '#6366f1';
  }
}
