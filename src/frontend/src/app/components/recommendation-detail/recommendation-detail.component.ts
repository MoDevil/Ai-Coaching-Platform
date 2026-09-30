import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RecommendationService } from '../../services/recommendation.service';
import { ClientService } from '../../services/client.service';
import { ProgramService } from '../../services/program.service';
import { Client } from '../../models/client.models';
import {
  AIRecommendationDetail,
  AIRecommendationCategoryLabels,
  AIRecommendationReviewStatus,
  AIRecommendationReviewStatusLabels,
  ReviewAIRecommendationRequest
} from '../../models/recommendation.models';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-recommendation-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './recommendation-detail.component.html',
  styleUrls: ['./recommendation-detail.component.css']
})
export class RecommendationDetailComponent implements OnInit {
  clientId: string = '';
  recommendationId: string = '';
  client: Client | null = null;
  detail: AIRecommendationDetail | null = null;

  isLoading: boolean = false;
  isSubmitting: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // Collapsible section toggles
  showRationale: boolean = false; // Collapsed by default
  showAssumptions: boolean = false;
  showMissingData: boolean = false;
  expandedClaims: Set<string> = new Set<string>();

  // Coach Decision Form
  decisionStatus: AIRecommendationReviewStatus = AIRecommendationReviewStatus.Accepted;
  coachDecision: string = '';
  finalImplementedPlan: string = '';
  formError: string = '';

  // Program Version Linking
  availableProgramVersions: Array<{ id: string; label: string }> = [];
  selectedProgramVersionId: string = '';
  isLinkingVersion: boolean = false;
  linkVersionError: string = '';
  linkVersionSuccess: string = '';

  readonly CategoryLabels = AIRecommendationCategoryLabels;
  readonly StatusLabels = AIRecommendationReviewStatusLabels;
  readonly ReviewStatusEnum = AIRecommendationReviewStatus;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recommendationService: RecommendationService,
    private clientService: ClientService,
    private programService: ProgramService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    this.recommendationId = this.route.snapshot.paramMap.get('recommendationId') || '';

    if (this.clientId && this.recommendationId) {
      this.loadClient();
      this.loadDetail();
    }
  }

  loadClient(): void {
    this.clientService.getClientById(this.clientId).subscribe({
      next: (client) => {
        this.client = client;
      },
      error: (err) => {
        console.error('Failed to load client profile', err);
      }
    });
  }

  loadDetail(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.recommendationService.getRecommendationDetail(this.clientId, this.recommendationId).subscribe({
      next: (data) => {
        this.detail = data;
        this.isLoading = false;

        // Initialize form fields from current state if available
        if (data.coachDecision) {
          this.coachDecision = data.coachDecision;
        }
        if (data.finalImplementedPlan) {
          this.finalImplementedPlan = data.finalImplementedPlan;
        }
        const currentStatusStr = String(data.reviewStatus);
        if (currentStatusStr === 'Accepted' || currentStatusStr === '3') {
          this.decisionStatus = AIRecommendationReviewStatus.Accepted;
          this.loadAvailableProgramVersions();
        } else if (currentStatusStr === 'Rejected' || currentStatusStr === '4') {
          this.decisionStatus = AIRecommendationReviewStatus.Rejected;
        } else if (currentStatusStr === 'Archived' || currentStatusStr === '5') {
          this.decisionStatus = AIRecommendationReviewStatus.Archived;
        } else if (currentStatusStr === 'UnderReview' || currentStatusStr === '2') {
          this.decisionStatus = AIRecommendationReviewStatus.UnderReview;
        } else {
          this.decisionStatus = AIRecommendationReviewStatus.Accepted;
        }

        if (data.implementedProgramVersionId) {
          this.selectedProgramVersionId = data.implementedProgramVersionId;
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to load recommendation detail.';
      }
    });
  }

  loadAvailableProgramVersions(): void {
    if (!this.clientId) return;
    this.programService.getProgramsByClientId(this.clientId).subscribe({
      next: (summaries) => {
        if (!summaries || summaries.length === 0) {
          this.availableProgramVersions = [];
          return;
        }
        const versions: Array<{ id: string; label: string }> = [];
        let loaded = 0;
        for (const summary of summaries) {
          this.programService.getProgramById(summary.id).subscribe({
            next: (program) => {
              if (program && program.versions) {
                for (const v of program.versions) {
                  versions.push({
                    id: v.id,
                    label: `v${v.versionNumber} - ${program.name}${v.isActive ? ' (Active)' : ''}`
                  });
                }
              }
              loaded++;
              if (loaded === summaries.length) {
                this.availableProgramVersions = versions;
              }
            },
            error: () => {
              loaded++;
              if (loaded === summaries.length) {
                this.availableProgramVersions = versions;
              }
            }
          });
        }
      },
      error: () => {
        this.availableProgramVersions = [];
      }
    });
  }

  onLinkProgramVersion(): void {
    if (!this.selectedProgramVersionId) {
      this.linkVersionError = 'Please select a program version to link.';
      return;
    }

    this.isLinkingVersion = true;
    this.linkVersionError = '';
    this.linkVersionSuccess = '';

    this.recommendationService.linkProgramVersion(this.recommendationId, {
      programVersionId: this.selectedProgramVersionId
    }).subscribe({
      next: () => {
        this.isLinkingVersion = false;
        this.linkVersionSuccess = 'Program version linked successfully.';
        this.loadDetail();
      },
      error: (err) => {
        this.isLinkingVersion = false;
        this.linkVersionError = err.error?.detail || err.error?.title || 'Failed to link program version.';
      }
    });
  }

  toggleClaim(claimId: string): void {
    if (this.expandedClaims.has(claimId)) {
      this.expandedClaims.delete(claimId);
    } else {
      this.expandedClaims.add(claimId);
    }
  }

  onSelectStatus(status: AIRecommendationReviewStatus): void {
    this.decisionStatus = status;
    this.formError = '';
  }

  onSubmitReview(): void {
    this.formError = '';
    this.errorMessage = '';
    this.successMessage = '';

    // Frontend validation according to state machine rules
    if (this.decisionStatus === AIRecommendationReviewStatus.Accepted) {
      if (!this.coachDecision.trim()) {
        this.formError = 'Coach decision note is required when accepting a recommendation.';
        return;
      }
    }

    if (this.decisionStatus !== AIRecommendationReviewStatus.Accepted && this.finalImplementedPlan.trim()) {
      this.formError = 'Final implemented plan can only be provided when accepting the recommendation.';
      return;
    }

    this.isSubmitting = true;

    const request: ReviewAIRecommendationRequest = {
      reviewStatus: this.decisionStatus,
      coachDecision: this.coachDecision.trim() || null,
      finalImplementedPlan: this.decisionStatus === AIRecommendationReviewStatus.Accepted ? (this.finalImplementedPlan.trim() || null) : null
    };

    this.recommendationService.reviewRecommendation(this.recommendationId, request).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.successMessage = 'Coach review recorded successfully.';
        this.loadDetail();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.formError = err.error?.detail || err.error?.title || 'Failed to apply review decision.';
      }
    });
  }

  getStatusBadgeClass(status: string | number): string {
    const s = String(status);
    switch (s) {
      case 'PendingReview':
      case '1':
        return 'badge-warning';
      case 'UnderReview':
      case '2':
        return 'badge-info';
      case 'Accepted':
      case '3':
        return 'badge-success';
      case 'Rejected':
      case '4':
        return 'badge-danger';
      case 'Archived':
      case '5':
        return 'badge-secondary';
      default:
        return 'badge-secondary';
    }
  }

  isArchived(): boolean {
    if (!this.detail) return false;
    const s = String(this.detail.reviewStatus);
    return s === 'Archived' || s === '5';
  }
}
