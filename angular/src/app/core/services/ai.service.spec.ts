import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { AiService, ChatMessage } from './ai.service';

/**
 * AiService uses rxjs `of(...).pipe(delay(...))` internally, so every
 * Observable must be ticked past the delay before asserting.
 *
 * askAi  → 600 ms delay
 * getDashboardAiAnalysis → 800 ms delay
 */
describe('AiService', () => {
  let service: AiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AiService);
  });

  // ─── Creation ─────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── askAi() – message shape ──────────────────────────────────────────────

  it('askAi() should return an Observable that emits exactly one ChatMessage', fakeAsync(() => {
    let emitted: ChatMessage | undefined;

    service.askAi('hello').subscribe(msg => (emitted = msg));
    tick(600);

    expect(emitted).toBeDefined();
    expect(emitted!.sender).toBe('ai');
  }));

  it('askAi() message should have a non-empty id prefixed with "msg-"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('any question').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.id).toMatch(/^msg-/);
  }));

  it('askAi() message should include a non-empty timestamp string', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('any question').subscribe(m => (msg = m));
    tick(600);

    expect(typeof msg!.timestamp).toBe('string');
    expect(msg!.timestamp.length).toBeGreaterThan(0);
  }));

  it('askAi() message should have sender set to "ai"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('test prompt').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.sender).toBe('ai');
  }));

  it('askAi() message should have a non-empty text property', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('test prompt').subscribe(m => (msg = m));
    tick(600);

    expect(typeof msg!.text).toBe('string');
    expect(msg!.text.length).toBeGreaterThan(0);
  }));

  // ─── askAi() – leave / workflow branch ───────────────────────────────────

  it('askAi() with leave + days keyword should return a generatedWorkflow', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('create a leave workflow for more than 5 days').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow).toBeDefined();
  }));

  it('askAi() with leave + workflow keyword should produce a workflow named correctly', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('show me the leave workflow').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.name).toBe('AI Generated Leave Approval Workflow');
  }));

  it('askAi() leave workflow should have status "Draft"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('leave workflow create').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.status).toBe('Draft');
  }));

  it('askAi() leave workflow should have 6 nodes', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('leave days workflow').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.nodes.length).toBe(6);
  }));

  it('askAi() leave workflow should have 6 connections', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('leave more than workflow').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.connections.length).toBe(6);
  }));

  it('askAi() leave workflow should have triggerType "Entity Created"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('create leave workflow').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.triggerType).toBe('Entity Created');
  }));

  it('askAi() leave workflow should have createdBy "AI Assistant"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('create leave workflow').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow!.createdBy).toBe('AI Assistant');
  }));

  it('askAi() leave workflow first node should be a trigger node', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('leave workflow days').subscribe(m => (msg = m));
    tick(600);

    const firstNode = msg!.generatedWorkflow!.nodes[0];
    expect(firstNode.type).toBe('trigger');
  }));

  it('askAi() leave workflow reply should mention "Leave Approval Workflow"', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('leave workflow days').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('Leave Approval Workflow');
  }));

  // ─── askAi() – employee / headcount branch ───────────────────────────────

  it('askAi() with "employee" keyword should NOT generate a workflow', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('How many employees do we have?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow).toBeUndefined();
  }));

  it('askAi() with "employee" keyword reply should mention headcount', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('show me employee count').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('245 active employees');
  }));

  it('askAi() with "headcount" keyword should return employee statistics', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('What is our headcount?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('245 active employees');
  }));

  it('askAi() with "joined" keyword should return employee statistics', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('Who joined this month?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('8 new employees');
  }));

  // ─── askAi() – inventory / stock branch ──────────────────────────────────

  it('askAi() with "inventory" keyword reply should mention inventory valuation', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('What is our inventory status?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('$125,450');
  }));

  it('askAi() with "stock" keyword reply should mention low stock items', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('Check our stock levels').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('12 products');
  }));

  it('askAi() with "product" keyword should NOT generate a workflow', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('List our products').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow).toBeUndefined();
  }));

  // ─── askAi() – approval / task / pending branch ──────────────────────────

  it('askAi() with "approval" keyword reply should mention pending approvals', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('Show my pending approvals').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('18 pending approvals');
  }));

  it('askAi() with "task" keyword reply should include approval queue details', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('What tasks do I have?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('18 pending approvals');
  }));

  it('askAi() with "pending" keyword reply should include turnaround time', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('What is pending?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('4.2 hours');
  }));

  // ─── askAi() – fallback / default branch ─────────────────────────────────

  it('askAi() with an unrecognised question should return the generic help reply', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('What is the weather like today?').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('ERP Platform AI Assistant');
  }));

  it('askAi() fallback reply should NOT include a generatedWorkflow', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('completely unrelated question about cats').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.generatedWorkflow).toBeUndefined();
  }));

  it('askAi() should be case-insensitive for keyword matching', fakeAsync(() => {
    let msg: ChatMessage | undefined;

    service.askAi('EMPLOYEE HEADCOUNT REPORT').subscribe(m => (msg = m));
    tick(600);

    expect(msg!.text).toContain('245 active employees');
  }));

  // ─── askAi() – Observable delay ──────────────────────────────────────────

  it('askAi() should NOT emit before the 600 ms delay elapses', fakeAsync(() => {
    let emitted = false;

    service.askAi('test').subscribe(() => (emitted = true));

    tick(599);
    expect(emitted).toBeFalse();

    tick(1);
    expect(emitted).toBeTrue();
  }));

  // ─── getDashboardAiAnalysis() ─────────────────────────────────────────────

  it('getDashboardAiAnalysis() should return an Observable that emits a string', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(typeof result).toBe('string');
    expect(result!.length).toBeGreaterThan(0);
  }));

  it('getDashboardAiAnalysis() should include the inventory section', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(result).toContain('Inventory Health');
  }));

  it('getDashboardAiAnalysis() should include the workforce section', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(result).toContain('Workforce Dynamics');
  }));

  it('getDashboardAiAnalysis() should include the workflow velocity section', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(result).toContain('Workflow Velocity');
  }));

  it('getDashboardAiAnalysis() should mention the inventory valuation figure', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(result).toContain('$125,450');
  }));

  it('getDashboardAiAnalysis() should mention the 96.4% attendance rate', fakeAsync(() => {
    let result: string | undefined;

    service.getDashboardAiAnalysis().subscribe(s => (result = s));
    tick(800);

    expect(result).toContain('96.4%');
  }));

  it('getDashboardAiAnalysis() should NOT emit before the 800 ms delay elapses', fakeAsync(() => {
    let emitted = false;

    service.getDashboardAiAnalysis().subscribe(() => (emitted = true));

    tick(799);
    expect(emitted).toBeFalse();

    tick(1);
    expect(emitted).toBeTrue();
  }));
});
