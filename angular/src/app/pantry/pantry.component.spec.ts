import '@angular/compiler';
import { DestroyRef, Injector, runInInjectionContext } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ListService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { PantryItemDto, PantryItemService } from '@proxy/pantry';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { NEVER, Observable, Subject, of, throwError } from 'rxjs';
import { PantryComponent } from './pantry.component';

class FakeListService {
  page = 0;
  maxResultCount = 10;
  totalCount = 0;
  private response = new Subject<{ items: PantryItemDto[]; totalCount: number }>();
  private query!: (input: { skipCount: number; maxResultCount: number; sorting: string }) =>
    Observable<{ items: PantryItemDto[]; totalCount: number }>;

  hookToQuery(callback: typeof this.query) {
    this.query = callback;
    return this.response.asObservable();
  }

  get = () => {
    this.query({ skipCount: this.page * this.maxResultCount, maxResultCount: this.maxResultCount, sorting: '' })
      .subscribe(result => this.response.next(result));
  };
}

describe('PantryComponent', () => {
  let list: FakeListService;
  let service: {
    getList: ReturnType<typeof vi.fn>;
    update: ReturnType<typeof vi.fn>;
    markConsumed: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };
  let confirmation: { warn: ReturnType<typeof vi.fn> };
  let component: PantryComponent;
  const item: PantryItemDto = {
    id: '20d83d8f-df47-48a5-a99a-958d4563c161',
    productId: '54269c76-0abc-46c7-900f-9bc1578daa7c',
    productName: 'Arroz', quantity: 2, unit: 'kg', expirationDate: '2026-12-31', isActive: true,
  };

  beforeEach(() => {
    list = new FakeListService();
    service = {
      getList: vi.fn(), update: vi.fn(), markConsumed: vi.fn(), delete: vi.fn(),
    };
    confirmation = { warn: vi.fn() };
    const injector = Injector.create({
      providers: [
        { provide: ListService, useValue: list },
        { provide: PantryItemService, useValue: service },
        { provide: ConfirmationService, useValue: confirmation },
        { provide: FormBuilder, useClass: FormBuilder },
        { provide: DestroyRef, useValue: { onDestroy: () => {} } },
      ],
    });
    component = runInInjectionContext(injector, () => new PantryComponent());
  });

  it('distinguishes pending, error, empty and results without a network call', () => {
    service.getList.mockReturnValueOnce(NEVER);
    list.get();
    expect(component.state()).toBe('loading');

    service.getList.mockReturnValueOnce(throwError(() => new Error('offline')));
    list.get();
    expect(component.state()).toBe('error');

    service.getList.mockReturnValueOnce(of({ items: [], totalCount: 0 }));
    list.get();
    expect(component.state()).toBe('empty');

    service.getList.mockReturnValueOnce(of({ items: [item], totalCount: 1 }));
    list.get();
    expect(component.state()).toBe('results');
    expect(component.items()[0].productName).toBe('Arroz');
  });

  it('does not submit invalid quantity or date', () => {
    component.edit(item);
    component.form.patchValue({ quantity: -1, expirationDate: '2026-02-31' });
    component.save();
    expect(component.form.invalid).toBe(true);
    expect(service.update).not.toHaveBeenCalled();
  });

  it('keeps form values when server validation rejects an update', () => {
    component.edit(item);
    component.form.patchValue({ quantity: 4 });
    service.update.mockReturnValue(throwError(() => ({ status: 400 })));
    component.save();
    expect(component.selected()?.id).toBe(item.id);
    expect(component.form.controls.quantity.value).toBe(4);
    expect(component.actionError()).toBe('::Pantry:ServerValidation');
  });

  it('marks a pantry item as consumed through the generated proxy', () => {
    service.markConsumed.mockReturnValue(of({ ...item, isActive: false }));
    service.getList.mockReturnValue(of({ items: [{ ...item, isActive: false }], totalCount: 1 }));
    component.consume(item);
    expect(service.markConsumed).toHaveBeenCalledWith(item.id, { skipHandleError: true });
  });

  it('does not call delete when confirmation is cancelled', () => {
    confirmation.warn.mockReturnValue(of(Confirmation.Status.reject));
    component.remove(item);
    expect(service.delete).not.toHaveBeenCalled();
  });

  it('deletes only after confirmation', () => {
    confirmation.warn.mockReturnValue(of(Confirmation.Status.confirm));
    service.delete.mockReturnValue(of(undefined));
    service.getList.mockReturnValue(of({ items: [], totalCount: 0 }));
    component.remove(item);
    expect(service.delete).toHaveBeenCalledWith(item.id, { skipHandleError: true });
  });
});
