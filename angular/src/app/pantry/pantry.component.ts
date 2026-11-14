import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ListService, LocalizationPipe } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { PantryItemDto, PantryItemService } from '@proxy/pantry';
import { catchError, of } from 'rxjs';

type ListState = 'loading' | 'error' | 'empty' | 'results';

const calendarDate: ValidatorFn = control => {
  const value = control.value as string | null;
  if (!value) return null;
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return { date: true };
  const [year, month, day] = value.split('-').map(Number);
  const parsed = new Date(Date.UTC(year, month - 1, day));
  const valid = parsed.getUTCFullYear() === year && parsed.getUTCMonth() + 1 === month &&
    parsed.getUTCDate() === day;
  return valid ? null : ({ date: true } as ValidationErrors);
};

@Component({
  selector: 'app-pantry',
  templateUrl: './pantry.component.html',
  styleUrl: './pantry.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, LocalizationPipe, DatePipe],
  providers: [ListService],
})
export class PantryComponent {
  readonly list = inject(ListService);
  private readonly service = inject(PantryItemService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly fb = inject(FormBuilder);

  readonly items = signal<PantryItemDto[]>([]);
  readonly state = signal<ListState>('loading');
  readonly selected = signal<PantryItemDto | null>(null);
  readonly saving = signal(false);
  readonly busyId = signal<string | null>(null);
  readonly actionError = signal<string | null>(null);

  readonly form = this.fb.group({
    quantity: [null as number | null, [Validators.min(0)]],
    unit: ['', [Validators.maxLength(32)]],
    expirationDate: ['', [calendarDate]],
  });

  constructor() {
    this.list.maxResultCount = 10;
    this.list.hookToQuery(query => {
      this.state.set('loading');
      return this.service.getList(query, { skipHandleError: true }).pipe(
        catchError(() => {
          this.state.set('error');
          return of({ items: [], totalCount: 0 });
        }),
      );
    }).pipe(takeUntilDestroyed()).subscribe(result => {
      if (this.state() === 'error') return;
      if (this.list.page > 0 && !result.items?.length && result.totalCount) {
        this.list.page = Math.floor((result.totalCount - 1) / this.list.maxResultCount);
        return;
      }
      this.items.set(result.items ?? []);
      this.list.totalCount = result.totalCount ?? 0;
      this.state.set(this.items().length === 0 ? 'empty' : 'results');
    });
  }

  retry(): void {
    this.list.get();
  }

  changePage(delta: number): void {
    const next = this.list.page + delta;
    if (next < 0 || next * this.list.maxResultCount >= this.list.totalCount) return;
    this.list.page = next;
  }

  edit(item: PantryItemDto): void {
    this.actionError.set(null);
    this.selected.set(item);
    this.form.reset({
      quantity: item.quantity ?? null,
      unit: item.unit ?? '',
      expirationDate: item.expirationDate ?? '',
    });
  }

  cancelEdit(): void {
    this.selected.set(null);
    this.actionError.set(null);
  }

  save(): void {
    const item = this.selected();
    if (!item?.id || this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.actionError.set(null);
    const value = this.form.getRawValue();
    this.service.update(item.id, {
      quantity: value.quantity,
      unit: value.unit?.trim() || null,
      expirationDate: value.expirationDate || null,
    }, { skipHandleError: true }).subscribe({
      next: () => {
        this.saving.set(false);
        this.selected.set(null);
        this.list.get();
      },
      error: error => {
        this.saving.set(false);
        this.actionError.set(error?.status === 400 ? '::Pantry:ServerValidation' : '::Pantry:SaveError');
      },
    });
  }

  consume(item: PantryItemDto): void {
    if (!item.id || this.busyId()) return;
    this.busyId.set(item.id);
    this.actionError.set(null);
    this.service.markConsumed(item.id, { skipHandleError: true }).subscribe({
      next: () => {
        this.busyId.set(null);
        this.list.get();
      },
      error: () => {
        this.busyId.set(null);
        this.actionError.set('::Pantry:ActionError');
      },
    });
  }

  remove(item: PantryItemDto): void {
    if (!item.id || this.busyId()) return;
    this.confirmation.warn('::Pantry:DeleteConfirmation', '::Pantry:DeleteTitle')
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;
        this.busyId.set(item.id!);
        this.actionError.set(null);
        this.service.delete(item.id!, { skipHandleError: true }).subscribe({
          next: () => {
            this.busyId.set(null);
            this.list.get();
          },
          error: () => {
            this.busyId.set(null);
            this.actionError.set('::Pantry:ActionError');
          },
        });
      });
  }
}
