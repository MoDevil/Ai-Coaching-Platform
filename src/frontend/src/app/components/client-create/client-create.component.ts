import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { CreateClientRequest, Gender } from '../../models/client.models';

@Component({
  selector: 'app-client-create',
  templateUrl: './client-create.component.html',
  styleUrls: ['./client-create.component.css']
})
export class ClientCreateComponent {
  clientForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  genders = [
    { value: Gender.Male, label: 'Male' },
    { value: Gender.Female, label: 'Female' },
    { value: Gender.Other, label: 'Other' }
  ];

  constructor(
    private fb: FormBuilder,
    private clientService: ClientService,
    private router: Router
  ) {
    this.clientForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.email, Validators.maxLength(256)]],
      phone: ['', [Validators.maxLength(50)]],
      dateOfBirth: [''],
      gender: [null],
      primaryGoal: ['', [Validators.maxLength(100)]],
      targetTimelineWeeks: [null, [Validators.min(1), Validators.max(104)]],
      goalDescription: ['', [Validators.maxLength(1000)]],
      intakeNotes: ['', [Validators.maxLength(2000)]]
    });
  }

  onSubmit(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const val = this.clientForm.value;

    const request: CreateClientRequest = {
      firstName: val.firstName.trim(),
      lastName: val.lastName.trim(),
      email: val.email?.trim() ? val.email.trim() : null,
      phone: val.phone?.trim() ? val.phone.trim() : null,
      dateOfBirth: val.dateOfBirth ? new Date(val.dateOfBirth).toISOString() : null,
      gender: val.gender ? Number(val.gender) : null,
      goal: val.primaryGoal?.trim() ? {
        primaryGoal: val.primaryGoal.trim(),
        targetTimelineWeeks: val.targetTimelineWeeks ? Number(val.targetTimelineWeeks) : null,
        description: val.goalDescription?.trim() ? val.goalDescription.trim() : null
      } : null,
      intakeNotes: val.intakeNotes?.trim() ? val.intakeNotes.trim() : null
    };

    this.clientService.createClient(request).subscribe({
      next: (created) => {
        this.isLoading = false;
        this.router.navigate(['/clients', created.id]);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || err.error?.detail || 'Failed to create client. Please check inputs.';
      }
    });
  }
}
