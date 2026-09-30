import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { RecommendationService } from '../../services/recommendation.service';
import {
  AIRecommendationSummary,
  AIRecommendationCategory,
  AIRecommendationCategoryLabels,
  AIRecommendationReviewStatus,
  AIRecommendationReviewStatusLabels
} from '../../models/recommendation.models';

@Component({
  selector: 'app-recommendation-history',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './recommendation-history.component.html',
  styleUrls: ['./recommendation-history.component.css']
})
export class RecommendationHistoryComponent implements OnInit, OnChanges {
  @Input() clientId!: string;

  recommendations: AIRecommendationSummary[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';

  selectedStatusFilter: string = 'all';
  selectedCategoryFilter: string = 'all';

  readonly CategoryLabels = AIRecommendationCategoryLabels;
  readonly StatusLabels = AIRecommendationReviewStatusLabels;
  readonly Categories = Object.keys(AIRecommendationCategory);
  readonly Statuses = Object.keys(AIRecommendationReviewStatus);

  constructor(private recommendationService: RecommendationService) {}

  ngOnInit(): void {
    if (this.clientId) {
      this.loadHistory();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['clientId'] && !changes['clientId'].firstChange) {
      this.loadHistory();
    }
  }

  loadHistory(): void {
    if (!this.clientId) return;

    this.isLoading = true;
    this.errorMessage = '';

    const statusParam = this.selectedStatusFilter === 'all' ? undefined : this.selectedStatusFilter;
    const catParam = this.selectedCategoryFilter === 'all' ? undefined : this.selectedCategoryFilter;

    this.recommendationService.getClientRecommendations(this.clientId, statusParam, catParam).subscribe({
      next: (data) => {
        this.recommendations = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to load recommendation history.';
      }
    });
  }

  onFilterChange(): void {
    this.loadHistory();
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
}
