import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
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
export class TriggerReasoningComponent {
  @Input() clientId!: string;
  @Output() generated = new EventEmitter<any>();

  isGenerating: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  selectedCategory: string = 'ProgramAdaptationReview';
  additionalContext: string = '';

  readonly CategoryLabels = AIRecommendationCategoryLabels;
  readonly Categories = Object.keys(AIRecommendationCategory);

  constructor(
    private recommendationService: RecommendationService,
    private router: Router
  ) {}

  onTriggerReasoning(): void {
    if (!this.selectedCategory || !this.clientId) return;

    this.isGenerating = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: GenerateReasoningRequest = {
      clientId: this.clientId,
      reasoningCategory: this.selectedCategory,
      additionalContext: this.additionalContext.trim() || null
    };

    this.recommendationService.generateReasoning(request).subscribe({
      next: (result) => {
        this.isGenerating = false;
        this.successMessage = 'New AI recommendation generated successfully!';
        this.additionalContext = '';
        this.generated.emit(result);
        if (result && result.id) {
          this.router.navigate(['/clients', this.clientId, 'recommendations', result.id]);
        }
      },
      error: (err) => {
        this.isGenerating = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to generate recommendation. Check safety flags or network.';
      }
    });
  }
}
