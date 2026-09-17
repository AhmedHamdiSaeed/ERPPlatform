import { Directive, Input, TemplateRef, ViewContainerRef, inject, effect } from '@angular/core';
import { StateService } from '../services/state.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private state = inject(StateService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);

  private hasView = false;
  private permissionValue = '';

  constructor() {
    effect(() => {
      // Re-evaluate whenever the currentUser signal in StateService changes
      const user = this.state.currentUser();
      this.updateView();
    });
  }

  @Input() set hasPermission(permission: string | string[] | undefined | null) {
    if (Array.isArray(permission)) {
      this.permissionValue = permission.join(',');
    } else {
      this.permissionValue = permission || '';
    }
    this.updateView();
  }

  private updateView() {
    if (!this.permissionValue) {
      this.showView();
      return;
    }

    const permissions = this.permissionValue.split(',').map(p => p.trim()).filter(Boolean);
    const isGranted = permissions.some(p => this.state.hasPermission(p));

    if (isGranted && !this.hasView) {
      this.showView();
    } else if (!isGranted && this.hasView) {
      this.clearView();
    }
  }

  private showView() {
    if (!this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    }
  }

  private clearView() {
    this.viewContainer.clear();
    this.hasView = false;
  }
}
