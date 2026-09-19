import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientService } from '../../services/client.service';
import {
  AddConsentRequest,
  Client,
  ClientStatus,
  ClientStatusLabels,
  GenderLabels
} from '../../models/client.models';

@Component({
  selector: 'app-client-detail',
  templateUrl: './client-detail.component.html',
  styleUrls: ['./client-detail.component.css']
})
export class ClientDetailComponent implements OnInit {
  clientId = '';
  client: Client | null = null;
  isLoading = true;
  errorMessage = '';
  successMessage = '';

  // Consent form state
  isSubmittingConsent = false;
  consentType = 'DataProcessing';
  consentGranted = true;
  consentNotes = '';
  consentError = '';

  ClientStatus = ClientStatus;
  ClientStatusLabels = ClientStatusLabels;
  GenderLabels = GenderLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clientService: ClientService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.clientId) {
      this.errorMessage = 'Client ID not provided.';
      this.isLoading = false;
      return;
    }
    this.loadClient();
  }

  loadClient(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.clientService.getClientById(this.clientId).subscribe({
      next: (data) => {
        this.client = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.status === 404
          ? 'Client not found or you do not have permission to view this client.'
          : 'Failed to load client details.';
      }
    });
  }

  onArchiveClient(): void {
    if (!confirm('Are you sure you want to archive this client? You can still view their history later.')) {
      return;
    }

    this.clientService.archiveClient(this.clientId).subscribe({
      next: () => {
        this.successMessage = 'Client archived successfully.';
        this.loadClient();
      },
      error: () => {
        this.errorMessage = 'Failed to archive client.';
      }
    });
  }

  onAddConsent(): void {
    if (!this.consentType.trim()) {
      this.consentError = 'Consent type is required.';
      return;
    }

    this.isSubmittingConsent = true;
    this.consentError = '';

    const request: AddConsentRequest = {
      consentType: this.consentType.trim(),
      isGranted: this.consentGranted,
      notes: this.consentNotes.trim() ? this.consentNotes.trim() : null
    };

    this.clientService.addConsentRecord(this.clientId, request).subscribe({
      next: (record) => {
        this.isSubmittingConsent = false;
        this.consentNotes = '';
        if (this.client) {
          this.client.consentRecords = [record, ...this.client.consentRecords];
        }
        this.successMessage = 'Consent record added.';
      },
      error: (err) => {
        this.isSubmittingConsent = false;
        this.consentError = 'Failed to record consent.';
      }
    });
  }

  getStatusBadgeClass(status: ClientStatus): string {
    return status === ClientStatus.Active ? 'badge-active' : 'badge-archived';
  }
}
