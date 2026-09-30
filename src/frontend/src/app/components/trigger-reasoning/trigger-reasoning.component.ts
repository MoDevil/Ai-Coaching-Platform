import { Component, EventEmitter, Input, OnDestroy, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { RecommendationService } from '../../services/recommendation.service';
import {
  AIRecommendationCategory,
  AIRecommendationCategoryLabels,
  GenerateReasoningRequest
} from '../../models/recommendation.models';

@Component({
  selector: 'app-trigger-reasoning',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './trigger-reasoning.component.html',
  styleUrls: ['./trigger-reasoning.component.css']
})
export class TriggerReasoningComponent implements OnDestroy {
  @Input() clientId!: string;
  @Output() generated = new EventEmitter<any>();

  isGenerating: boolean = false;
  elapsedSeconds: number = 0;
  timerInterval: any = null;
  activeSubscription?: Subscription;

  errorMessage: string = '';
  failedProvider: string = '';
  successMessage: string = '';
  lastProviderUsed: string = '';

  selectedCategory: string = 'ProgramAdaptationReview';
  additionalContext: string = '';

  readonly CategoryLabels = AIRecommendationCategoryLabels;
  readonly Categories = Object.keys(AIRecommendationCategory);

  constructor(
    private recommendationService: RecommendationService,
    private router: Router
  ) {}

  ngOnDestroy(): void {
    this.stopTimer();
    this.activeSubscription?.unsubscribe();
  }

  onTriggerReasoning(): void {
    if (!this.selectedCategory || !this.clientId) return;

    this.isGenerating = true;
    this.errorMessage = '';
    this.failedProvider = '';
    this.successMessage = '';
    this.lastProviderUsed = '';
    this.elapsedSeconds = 0;

    this.startTimer();

    const request: GenerateReasoningRequest = {
      clientId: this.clientId,
      reasoningCategory: this.selectedCategory,
      additionalContext: this.additionalContext.trim() || null
    };

    this.activeSubscription?.unsubscribe();
    this.activeSubscription = this.recommendationService.generateReasoning(request).subscribe({
      next: (result) => {
        this.stopTimer();
        this.isGenerating = false;
        this.lastProviderUsed = result?.aiProvider || result?.providerUsed || 'System';
        this.successMessage = `New AI recommendation generated via ${this.lastProviderUsed}!`;
        this.additionalContext = '';
        this.generated.emit(result);
        if (result && result.id) {
          this.router.navigate(['/clients', this.clientId, 'recommendations', result.id]);
        }
      },
      error: (err) => {
        this.stopTimer();
        this.isGenerating = false;
        this.failedProvider = err.error?.provider || err.error?.providerName || 'AI Provider';
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to generate recommendation. Check safety flags or provider status.';
      }
    });
  }

  onCancel(): void {
    this.stopTimer();
    this.activeSubscription?.unsubscribe();
    this.isGenerating = false;
    this.errorMessage = 'Reasoning process was cancelled by the coach.';
  }

  onRetry(): void {
    this.onTriggerReasoning();
  }

  private startTimer(): void {
    this.stopTimer();
    this.timerInterval = setInterval(() => {
      this.elapsedSeconds++;
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }
}
