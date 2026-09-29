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
  IngestionContentType,
  IngestionStatus,
  ClaimNature,
  ExpertClaimReviewStatus,
  CredibilityTier,
  ExpertPlatform,
  IngestionContentTypeLabels,
  IngestionStatusLabels,
  ClaimNatureLabels,
  ExpertClaimReviewStatusLabels,
  CredibilityTierLabels,
  ExpertPlatformLabels
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
  submitContentType: IngestionContentType = IngestionContentType.YouTube;

  // Source creation form
  showSourceModal = false;
  newSourceName = '';
  newSourceChannel = '';
  newSourcePlatform: ExpertPlatform = ExpertPlatform.YouTube;
  newSourceDomain = '';
  newSourceCredibility: CredibilityTier = CredibilityTier.High;
  newSourceBio = '';

  // Claim review modal / form
  selectedClaimForReview: ExpertClaimDto | null = null;
  reviewDecision: ExpertClaimReviewStatus = ExpertClaimReviewStatus.Approved;
  reviewNotes = '';
  reviewCreateNewKnowledgeClaim = true;
  reviewQuestion = '';
  reviewPractitionerNotes = '';
  reviewEgyptSpecificNotes = '';

  // Labels for template
  contentTypeLabels = IngestionContentTypeLabels;
  statusLabels = IngestionStatusLabels;
  natureLabels = ClaimNatureLabels;
  reviewStatusLabels = ExpertClaimReviewStatusLabels;
  credibilityLabels = CredibilityTierLabels;
  platformLabels = ExpertPlatformLabels;

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
      error: (err) => {
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
      error: (err) => {
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
      title: this.submitTitle.trim() || undefined,
      sourceId: this.submitSourceId || undefined,
      contentType: this.submitContentType
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
    if (!this.newSourceName.trim() || !this.newSourceChannel.trim() || !this.newSourceDomain.trim()) {
      this.errorMessage = 'Please complete all required fields for the expert source.';
      return;
    }

    const payload: CreateExpertSourceDto = {
      name: this.newSourceName.trim(),
      channelOrPublication: this.newSourceChannel.trim(),
      platform: this.newSourcePlatform,
      primaryDomain: this.newSourceDomain.trim(),
      credibilityTier: this.newSourceCredibility,
      bio: this.newSourceBio.trim() || null
    };

    this.ingestionService.createSource(payload).subscribe({
      next: (source) => {
        this.showSourceModal = false;
        this.newSourceName = '';
        this.newSourceChannel = '';
        this.newSourceDomain = '';
        this.newSourceBio = '';
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
    this.reviewDecision = ExpertClaimReviewStatus.Approved;
    this.reviewNotes = claim.coachNotes || '';
    this.reviewCreateNewKnowledgeClaim = claim.supportingClaimId == null;
    this.reviewQuestion = `What does expert consensus assert regarding ${claim.topic}?`;
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
        this.successMessage = `Claim review saved (${this.reviewStatusLabels[updatedClaim.reviewStatus]}).`;
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
