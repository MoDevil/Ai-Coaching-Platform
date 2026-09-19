import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { Gender, UpdateClientRequest } from '../../models/client.models';

@Component({
  selector: 'app-client-edit',
  templateUrl: './client-edit.component.html',
  styleUrls: ['./client-edit.component.css']
})
export class ClientEditComponent implements OnInit {
  clientId = '';
  clientForm: FormGroup;
  isLoading = true;
  isSaving = false;
  errorMessage = '';

  genders = [
    { value: Gender.Male, label: 'Male' },
    { value: Gender.Female, label: 'Female' },
    { value: Gender.Other, label: 'Other' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private clientService: ClientService
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

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.clientId) {
      this.errorMessage = 'Client ID missing.';
      this.isLoading = false;
      return;
    }
    this.loadClient();
  }

  loadClient(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.clientService.getClientById(this.clientId).subscribe({
      next: (client) => {
        let dobStr = '';
        if (client.dateOfBirth) {
          dobStr = client.dateOfBirth.substring(0, 10);
        }

        this.clientForm.patchValue({
          firstName: client.firstName,
          lastName: client.lastName,
          email: client.email || '',
          phone: client.phone || '',
          dateOfBirth: dobStr,
          gender: client.gender !== null && client.gender !== undefined ? client.gender : null,
          primaryGoal: client.goal?.primaryGoal || '',
          targetTimelineWeeks: client.goal?.targetTimelineWeeks || null,
          goalDescription: client.goal?.description || '',
          intakeNotes: client.intakeNotes || ''
        });
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load client for editing.';
      }
    });
  }

  onSubmit(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const val = this.clientForm.value;

    const request: UpdateClientRequest = {
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

    this.clientService.updateClient(this.clientId, request).subscribe({
      next: () => {
        this.isSaving = false;
        this.router.navigate(['/clients', this.clientId]);
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || err.error?.detail || 'Failed to update client.';
      }
    });
  }
}
