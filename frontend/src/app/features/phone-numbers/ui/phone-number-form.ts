import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatError, MatFormField, MatHint, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatRadioButton, MatRadioGroup } from '@angular/material/radio';
import { CreatePhoneNumberRequest, FieldErrors, Visibility } from '../data-access/phone-number';
import {
  CONTACT_NAME_MAX_LENGTH,
  PHONE_NUMBER_MAX_INPUT_LENGTH,
  contactNameValidator,
  phoneNumberValidator,
} from '../validation/phone-number-validators';

@Component({
  selector: 'app-phone-number-form',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatHint,
    MatError,
    MatInput,
    MatRadioGroup,
    MatRadioButton,
    MatButton,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form class="form" aria-label="Add phone number" [formGroup]="form" (ngSubmit)="submit()">
      <mat-form-field appearance="outline">
        <mat-label>Contact name</mat-label>
        <input matInput formControlName="contactName" autocomplete="off" />
        @if (form.controls.contactName.hasError('required')) {
          <mat-error>Contact name is required.</mat-error>
        } @else if (form.controls.contactName.hasError('maxlength')) {
          <mat-error>Contact name must be at most {{ contactNameMaxLength }} characters.</mat-error>
        } @else if (form.controls.contactName.hasError('server')) {
          <mat-error>{{ form.controls.contactName.getError('server') }}</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Phone number</mat-label>
        <input matInput formControlName="number" type="tel" autocomplete="off" />
        <mat-hint>For example +420 601 234 567</mat-hint>
        @if (form.controls.number.hasError('required')) {
          <mat-error>Phone number is required.</mat-error>
        } @else if (form.controls.number.hasError('maxlength')) {
          <mat-error>Phone number must be at most {{ numberMaxLength }} characters.</mat-error>
        } @else if (form.controls.number.hasError('phoneNumber')) {
          <mat-error
            >Enter a valid phone number: optional leading +, then 3 to 15 digits.</mat-error
          >
        } @else if (form.controls.number.hasError('server')) {
          <mat-error>{{ form.controls.number.getError('server') }}</mat-error>
        }
      </mat-form-field>

      <div class="visibility">
        <span id="visibility-label" class="visibility-label">Visibility</span>
        <mat-radio-group formControlName="visibility" aria-labelledby="visibility-label">
          <mat-radio-button value="PERSONAL">Personal</mat-radio-button>
          <mat-radio-button value="SHARED">Shared</mat-radio-button>
        </mat-radio-group>
        <p class="visibility-hint">
          {{
            form.controls.visibility.value === 'SHARED'
              ? 'Everyone who signs in can see this entry.'
              : 'Only you can see this entry.'
          }}
        </p>
        @if (form.controls.visibility.hasError('server')) {
          <p class="error" role="alert">{{ form.controls.visibility.getError('server') }}</p>
        }
      </div>

      @if (failed()) {
        <p class="error" role="alert">The phone number could not be saved. Try again later.</p>
      }

      <button matButton="filled" type="submit" [disabled]="saving()">Add</button>
    </form>
  `,
  styles: `
    .form {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .visibility {
      display: flex;
      flex-direction: column;
      margin-bottom: 0.75rem;
    }

    .visibility-label {
      font: var(--mat-sys-label-large);
      color: var(--mat-sys-on-surface-variant);
    }

    .visibility-hint {
      margin: 0;
      font: var(--mat-sys-body-small);
      color: var(--mat-sys-on-surface-variant);
    }

    .error {
      margin: 0.25rem 0 0;
      font: var(--mat-sys-body-small);
      color: var(--mat-sys-error);
    }

    button {
      align-self: flex-start;
    }
  `,
})
export class PhoneNumberForm {
  readonly saving = input(false);
  readonly failed = input(false);
  readonly serverErrors = input<FieldErrors>({});
  readonly submitted = output<CreatePhoneNumberRequest>();

  private readonly formDirective = viewChild.required(FormGroupDirective);

  protected readonly contactNameMaxLength = CONTACT_NAME_MAX_LENGTH;
  protected readonly numberMaxLength = PHONE_NUMBER_MAX_INPUT_LENGTH;

  protected readonly form = inject(NonNullableFormBuilder).group({
    contactName: ['', contactNameValidator],
    number: ['', phoneNumberValidator],
    visibility: ['PERSONAL' as Visibility],
  });

  constructor() {
    effect(() => {
      const errors = this.serverErrors();
      for (const field of ['contactName', 'number', 'visibility'] as const) {
        const message = errors[field];
        if (message !== undefined) {
          const control = this.form.controls[field];
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
    });
  }

  reset(): void {
    this.formDirective().resetForm({ contactName: '', number: '', visibility: 'PERSONAL' });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { contactName, number, visibility } = this.form.getRawValue();
    this.submitted.emit({ contactName: contactName.trim(), number: number.trim(), visibility });
  }
}
