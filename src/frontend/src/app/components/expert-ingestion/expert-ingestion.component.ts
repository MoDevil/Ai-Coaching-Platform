import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription, interval } from 'rxjs';
import { ExpertIngestionService } from '../../services/expert-ingestion.service';
import {
  ExpertContentIngestionSummaryDto,
  ExpertContentIngestionDto,
  ExpertSourceDto,
  CreateExpertSourceDto,
  SubmitIngestionRequestDto,
  ReviewClaimRequestDto,
  ExpertClaimDto,
  ExpertSourceType,
  IngestionSourceType,
  IngestionStatus,
  ClaimCategory,
  EvidenceClassification,
  CoachReviewStatus,
  CreatorConfidence,
  ExpertSourceTypeLabels,
  IngestionSourceTypeLabels,
  IngestionStatusLabels,
  ClaimCategoryLabels,
  EvidenceClassificationLabels,
  CoachReviewStatusLabels,
  CreatorConfidenceLabels
} from '../../models/expert-ingestion.models';

@Component({
  selector: 'app-expert-ingestion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './expert-ingestion.component.html',
  styleUrls: ['./expert-ingestion.component.css']
})
export class ExpertIngestionComponent implements OnInit, OnDestroy {
  ingestions: ExpertContentIngestionSummaryDto[] = [];
  sources: ExpertSourceDto[] = [];
  selectedIngestion: ExpertContentIngestionDto | null = null;

  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  // Ingestion submission form
  showSubmitModal = false;
  submitUrl = '';
  submitTitle = '';
  submitSourceId: string | null = null;
  submitSourceType: IngestionSourceType = IngestionSourceType.YouTubeVideo;

  // Source creation form
  showSourceModal = false;
  newSourceName = '';
  newSourceType: ExpertSourceType = ExpertSourceType.YouTubeChannel;
  newSourceUrl = '';

  // Claim review modal / form
  selectedClaimForReview: ExpertClaimDto | null = null;
  reviewDecision: CoachReviewStatus = CoachReviewStatus.Approved;
  reviewNotes = '';
  reviewCreateNewKnowledgeClaim = true;
  reviewQuestion = '';
  reviewPractitionerNotes = '';
  reviewEgyptSpecificNotes = '';

  // Labels for template
  sourceTypeLabels = ExpertSourceTypeLabels;
  ingestionSourceTypeLabels = IngestionSourceTypeLabels;
  statusLabels = IngestionStatusLabels;
  categoryLabels = ClaimCategoryLabels;
  evidenceLabels = EvidenceClassificationLabels;
  reviewStatusLabels = CoachReviewStatusLabels;
  confidenceLabels = CreatorConfidenceLabels;

  private pollSub?: Subscription;

  constructor(private ingestionService: ExpertIngestionService) {}

  ngOnInit(): void {
    this.loadIngestions();
    this.loadSources();
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }

  loadIngestions(): void {
    this.isLoading = true;
    this.ingestionService.getIngestions().subscribe({
      next: (data) => {
        this.ingestions = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed loading expert ingestions.';
        this.isLoading = false;
      }
    });
  }

  loadSources(): void {
    this.ingestionService.getSources().subscribe({
      next: (data) => {
        this.sources = data;
      },
      error: () => {
        // Non-fatal
      }
    });
  }

  selectIngestion(id: string): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = true;
    this.ingestionService.getIngestionById(id).subscribe({
      next: (data) => {
        this.selectedIngestion = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed loading ingestion details.';
        this.isLoading = false;
      }
    });
  }

  onSubmitIngestion(): void {
    if (!this.submitUrl.trim()) {
      this.errorMessage = 'Please provide a valid content URL.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const payload: SubmitIngestionRequestDto = {
      sourceUrl: this.submitUrl.trim(),
      sourceTitle: this.submitTitle.trim() || undefined,
      expertSourceId: this.submitSourceId || undefined,
      sourceType: this.submitSourceType
    };

    this.ingestionService.submitIngestion(payload).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        this.showSubmitModal = false;
        this.submitUrl = '';
        this.submitTitle = '';
        this.submitSourceId = null;
        this.successMessage = 'Content ingestion queued. Processing started in the background.';
        this.loadIngestions();
        this.selectIngestion(result.id);
      },
      error: (err) => {
        this.isSubmitting = false;
        if (err.status === 409) {
          this.errorMessage = 'An ingestion for this URL already exists for your coach account.';
        } else {
          this.errorMessage = err.error?.message || 'Failed submitting content ingestion.';
        }
      }
    });
  }

  onCreateSource(): void {
    if (!this.newSourceName.trim() || !this.newSourceUrl.trim()) {
      this.errorMessage = 'Please complete all required fields for the expert source.';
      return;
    }

    const payload: CreateExpertSourceDto = {
      name: this.newSourceName.trim(),
      sourceType: this.newSourceType,
      url: this.newSourceUrl.trim()
    };

    this.ingestionService.createSource(payload).subscribe({
      next: (source) => {
        this.showSourceModal = false;
        this.newSourceName = '';
        this.newSourceUrl = '';
        this.successMessage = `Expert source '${source.name}' created.`;
        this.loadSources();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed creating expert source.';
      }
    });
  }

  openReviewModal(claim: ExpertClaimDto): void {
    this.selectedClaimForReview = claim;
    this.reviewDecision = CoachReviewStatus.Approved;
    this.reviewNotes = claim.coachNote || '';
    this.reviewCreateNewKnowledgeClaim = claim.supportingClaimId == null;
    this.reviewQuestion = `What does expert consensus assert regarding ${this.categoryLabels[claim.claimCategory]}?`;
    this.reviewPractitionerNotes = '';
    this.reviewEgyptSpecificNotes = '';
  }

  onSaveClaimReview(): void {
    if (!this.selectedIngestion || !this.selectedClaimForReview) return;

    const ingestionId = this.selectedIngestion.id;
    const claimId = this.selectedClaimForReview.id;

    const payload: ReviewClaimRequestDto = {
      decision: this.reviewDecision,
      notes: this.reviewNotes.trim() || null,
      createNewKnowledgeClaim: this.reviewCreateNewKnowledgeClaim,
      newClaimQuestion: this.reviewQuestion.trim() || null,
      practitionerNotes: this.reviewPractitionerNotes.trim() || null,
      egyptSpecificNotes: this.reviewEgyptSpecificNotes.trim() || null
    };

    this.ingestionService.reviewClaim(ingestionId, claimId, payload).subscribe({
      next: (updatedClaim) => {
        this.selectedClaimForReview = null;
        this.successMessage = `Claim review saved (${this.reviewStatusLabels[updatedClaim.coachReviewStatus]}).`;
        this.selectIngestion(ingestionId);
        this.loadIngestions();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed updating claim review status.';
      }
    });
  }

  private startPolling(): void {
    this.pollSub = interval(5000).subscribe(() => {
      if (this.selectedIngestion && this.selectedIngestion.status === IngestionStatus.Processing) {
        this.selectIngestion(this.selectedIngestion.id);
        this.loadIngestions();
      }
    });
  }
}
