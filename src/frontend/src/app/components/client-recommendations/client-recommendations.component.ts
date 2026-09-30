import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RecommendationService } from '../../services/recommendation.service';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client.models';
import {
  AIRecommendationSummary,
  AIRecommendationCategory,
  AIRecommendationCategoryLabels,
  AIRecommendationReviewStatus,
  AIRecommendationReviewStatusLabels,
  GenerateReasoningRequest
} from '../../models/recommendation.models';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-client-recommendations',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './client-recommendations.component.html',
  styleUrls: ['./client-recommendations.component.css']
})
export class ClientRecommendationsComponent implements OnInit {
  clientId: string = '';
  client: Client | null = null;
  recommendations: AIRecommendationSummary[] = [];
  filteredRecommendations: AIRecommendationSummary[] = [];

  isLoading: boolean = false;
  isGenerating: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // Trigger form
  showTriggerForm: boolean = false;
  selectedCategory: string = 'ProgramAdaptationReview';
  additionalContext: string = '';

  // Filters
  selectedStatusFilter: string = 'all';
  selectedCategoryFilter: string = 'all';

  readonly CategoryLabels = AIRecommendationCategoryLabels;
  readonly StatusLabels = AIRecommendationReviewStatusLabels;
  readonly Categories = Object.keys(AIRecommendationCategory);
  readonly Statuses = Object.keys(AIRecommendationReviewStatus);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private recommendationService: RecommendationService,
    private clientService: ClientService
  ) {}

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (this.clientId) {
      this.loadClient();
      this.loadRecommendations();
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

  loadRecommendations(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const statusParam = this.selectedStatusFilter === 'all' ? undefined : this.selectedStatusFilter;
    const catParam = this.selectedCategoryFilter === 'all' ? undefined : this.selectedCategoryFilter;

    this.recommendationService.getClientRecommendations(this.clientId, statusParam, catParam).subscribe({
      next: (data) => {
        this.recommendations = data;
        this.filteredRecommendations = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to load recommendations.';
      }
    });
  }

  onFilterChange(): void {
    this.loadRecommendations();
  }

  onTriggerReasoning(): void {
    if (!this.selectedCategory) return;

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
        this.showTriggerForm = false;
        this.loadRecommendations();
      },
      error: (err) => {
        this.isGenerating = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Failed to generate recommendation. Check safety flags or network.';
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
}
