import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { WorkflowNode, WorkflowConnection, WorkflowDefinition } from '../models/erp-models';

export interface ChatMessage {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  timestamp: string;
  generatedWorkflow?: WorkflowDefinition;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {

  askAi(question: string): Observable<ChatMessage> {
    const q = question.toLowerCase();
    let reply = '';
    let generatedWf: WorkflowDefinition | undefined = undefined;

    if (q.includes('leave') && (q.includes('more than') || q.includes('days') || q.includes('workflow') || q.includes('create'))) {
      reply = `I have generated a custom Leave Approval Workflow based on your request:\n\n1. **Trigger**: Employee Leave Submitted\n2. **Condition**: Leave Days > 5\n3. **If Yes**: Manager Approval + HR Director Approval\n4. **If No**: Direct Manager Approval\n5. **Notification**: Send Email & In-App Notice\n\nYou can inspect or import this workflow below:`;
      
      generatedWf = {
        id: `ai-wf-${Date.now()}`,
        name: 'AI Generated Leave Approval Workflow',
        description: 'Auto-generated workflow for leave requests based on natural language requirements.',
        version: 'v1.0-AI',
        triggerType: 'Entity Created',
        status: 'Draft',
        createdBy: 'AI Assistant',
        createdDate: new Date().toISOString().split('T')[0],
        nodes: [
          { id: 'ain-1', type: 'trigger', title: 'Leave Submitted', subtitle: 'On Leave Created', x: 100, y: 150 },
          { id: 'ain-2', type: 'condition', title: 'Days > 5?', subtitle: 'Leave Days Condition', x: 320, y: 150 },
          { id: 'ain-3', type: 'approval', title: 'Manager Approval', subtitle: 'Line Manager', x: 550, y: 80 },
          { id: 'ain-4', type: 'approval', title: 'HR + Director Approval', subtitle: 'HR Review', x: 550, y: 240 },
          { id: 'ain-5', type: 'notification', title: 'Send Notifications', subtitle: 'Email & In-App', x: 800, y: 150 },
          { id: 'ain-6', type: 'end', title: 'Complete', subtitle: 'Done', x: 1000, y: 150 }
        ],
        connections: [
          { id: 'aic-1', sourceId: 'ain-1', targetId: 'ain-2' },
          { id: 'aic-2', sourceId: 'ain-2', targetId: 'ain-3', label: 'No' },
          { id: 'aic-3', sourceId: 'ain-2', targetId: 'ain-4', label: 'Yes' },
          { id: 'aic-4', sourceId: 'ain-3', targetId: 'ain-5' },
          { id: 'aic-5', sourceId: 'ain-4', targetId: 'ain-5' },
          { id: 'aic-6', sourceId: 'ain-5', targetId: 'ain-6' }
        ]
      };
    } else if (q.includes('employee') || q.includes('headcount') || q.includes('joined')) {
      reply = `According to current HR records:\n• Total Headcount: **245 active employees** across 7 departments.\n• **8 new employees** joined this month.\n• Largest department: **Supply Chain & Logistics** (65 staff).\n• Attendance rate today: **96.4%**.`;
    } else if (q.includes('inventory') || q.includes('stock') || q.includes('product')) {
      reply = `Inventory Analysis Summary:\n• Total Inventory Valuation: **$125,450**\n• Low Stock Items: **12 products** (e.g. Logitech MX Master 3S, Industrial Motors)\n• Out of Stock: **1 item** (Cisco Switch)\n• Warehouse Capacity: Main Warehouse is at **78% capacity**.`;
    } else if (q.includes('approval') || q.includes('task') || q.includes('pending')) {
      reply = `You have **18 pending approvals** in your queue:\n• 8 Leave Requests\n• 6 Purchase Orders\n• 4 Stock Transfers\n\nAverage approval turnaround time is currently **4.2 hours**.`;
    } else {
      reply = `I am your ERP Platform AI Assistant. I can help you analyze workforce data, inspect inventory levels, summarize pending workflow approvals, or generate custom workflow visual diagrams automatically. Try asking:\n- *"Show employees who joined this month"* \n- *"Explain inventory stock level changes"* \n- *"When an employee requests leave > 5 days send to HR"*`;
    }

    const aiMsg: ChatMessage = {
      id: `msg-${Date.now()}`,
      sender: 'ai',
      text: reply,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      generatedWorkflow: generatedWf
    };

    return of(aiMsg).pipe(delay(600));
  }

  getDashboardAiAnalysis(): Observable<string> {
    const summary = `📊 **AI Executive ERP Summary**:

1. **Inventory Health**: Overall inventory value grew by +4.5% to $125,450. However, 12 electronics items have hit critical reorder levels. Recommend placing a PO with TechSupply Co.

2. **Workforce Dynamics**: Headcount increased by 8 members this month (+3.3%). Employee attendance stands high at 96.4% with 0.8h average daily overtime.

3. **Workflow Velocity**: Workflow execution throughput is up 14%. Average approval turnaround time dropped to 4.2h, though Purchase Orders > $5k experience a 12h delay at CFO review.`;
    return of(summary).pipe(delay(800));
  }
}
