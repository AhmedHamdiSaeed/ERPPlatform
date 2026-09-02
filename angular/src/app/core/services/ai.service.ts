import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  WorkflowConnection,
  WorkflowDefinition,
  WorkflowNode,
  WorkflowNodeType,
} from '../models/erp-models';

export interface ChatMessage {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  timestamp: string;
  generatedWorkflow?: WorkflowDefinition;
}

interface AiAskResponse {
  answer: string;
  generatedWorkflowJson: string;
  timestamp: string;
  sessionId: string;
}

@Injectable({
  providedIn: 'root',
})
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apis.default.url}/api/ai/ai-assistant`;

  // One session per browser tab so the backend keeps conversation history.
  private sessionId = `web-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;

  askAi(question: string): Observable<ChatMessage> {
    const body = { prompt: question, sessionId: this.sessionId, context: '' };

    return this.http.post<AiAskResponse>(`${this.baseUrl}/ask`, body).pipe(
      map((res) => this.toChatMessage(res)),
      catchError(() =>
        of(this.errorMessage('Sorry, the AI service is currently unavailable. Please try again later.')),
      ),
    );
  }

  getDashboardAiAnalysis(): Observable<string> {
    return this.http
      .get(`${this.baseUrl}/executive-summary`, { responseType: 'text' })
      .pipe(
        catchError(() =>
          of(
            'AI Executive Summary is currently unavailable. Configure the AI provider in the backend to enable live insights.',
          ),
        ),
      );
  }

  private toChatMessage(res: AiAskResponse): ChatMessage {
    return {
      id: `msg-${Date.now()}`,
      sender: 'ai',
      text: res.answer,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      generatedWorkflow: this.parseWorkflow(res.generatedWorkflowJson),
    };
  }

  private errorMessage(text: string): ChatMessage {
    return {
      id: `msg-${Date.now()}`,
      sender: 'ai',
      text,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };
  }

  // The backend returns a workflow graph as JSON (either from the model or a
  // fallback sketch). Turn it into a renderable WorkflowDefinition.
  private parseWorkflow(json: string | undefined): WorkflowDefinition | undefined {
    if (!json) return undefined;
    try {
      const parsed = JSON.parse(json);
      const nodes: any[] = parsed?.nodes;
      if (!Array.isArray(nodes) || nodes.length === 0) return undefined;

      const validTypes: string[] = [
        'trigger', 'action', 'condition', 'approval', 'notification',
        'delay', 'webhook', 'api_request', 'ai', 'end',
      ];

      const wfNodes: WorkflowNode[] = nodes.map((n, i) => ({
        id: String(n.id ?? `n-${i}`),
        type: validTypes.includes(n.type) ? (n.type as WorkflowNodeType) : this.guessNodeType(String(n.title ?? '')),
        title: String(n.title ?? `Step ${i + 1}`),
        subtitle: n.subtitle ? String(n.subtitle) : undefined,
        x: 100 + i * 220,
        y: 150,
      }));

      const rawConns: any[] = Array.isArray(parsed?.connections) ? parsed.connections : [];
      const wfConns: WorkflowConnection[] = rawConns
        .map((c, i) => ({
          id: `c-${i}`,
          sourceId: String(c.from ?? c.sourceId ?? ''),
          targetId: String(c.to ?? c.targetId ?? ''),
          label: c.label ? String(c.label) : undefined,
        }))
        .filter((c) => c.sourceId && c.targetId);

      return {
        id: `ai-wf-${Date.now()}`,
        name: parsed?.name ?? 'AI Generated Workflow',
        description: parsed?.description ?? 'Auto-generated workflow from your natural language request.',
        version: 'v1.0-AI',
        triggerType: 'Entity Created',
        status: 'Draft',
        createdBy: 'AI Assistant',
        createdDate: new Date().toISOString().split('T')[0],
        nodes: wfNodes,
        connections: wfConns,
      };
    } catch {
      return undefined;
    }
  }

  private guessNodeType(title: string): WorkflowNodeType {
    const t = title.toLowerCase();
    if (t.includes('trigger') || t.includes('submit')) return 'trigger';
    if (t.includes('approv')) return 'approval';
    if (t.includes('condition') || t.includes('?') || t.includes('if ')) return 'condition';
    if (t.includes('notif') || t.includes('email') || t.includes('message')) return 'notification';
    return 'action';
  }
}
