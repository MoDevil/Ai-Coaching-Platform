import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ExerciseService } from '../../services/exercise.service';
import { KnowledgeService } from '../../services/knowledge.service';
import {
  ExerciseCategoryLabels,
  ExerciseDetail,
  MetadataStatus,
  MetadataStatusLabels,
  QualitativeRating,
  QualitativeRatingLabels,
  ResistanceProfileLabels
} from '../../models/exercise.models';
import {
  EvidenceLevelLabels,
  KnowledgeClaimSummary
} from '../../models/knowledge.models';

@Component({
  selector: 'app-exercise-detail',
  templateUrl: './exercise-detail.component.html',
  styleUrls: ['./exercise-detail.component.css']
})
export class ExerciseDetailComponent implements OnInit {
  exerciseId = '';
  exercise: ExerciseDetail | null = null;
  claims: KnowledgeClaimSummary[] = [];
  isLoading = true;
  errorMessage = '';

  ExerciseCategoryLabels = ExerciseCategoryLabels;
  ResistanceProfileLabels = ResistanceProfileLabels;
  QualitativeRatingLabels = QualitativeRatingLabels;
  MetadataStatus = MetadataStatus;
  MetadataStatusLabels = MetadataStatusLabels;
  EvidenceLevelLabels = EvidenceLevelLabels;

  constructor(
    private route: ActivatedRoute,
    private exerciseService: ExerciseService,
    private knowledgeService: KnowledgeService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.exerciseId = params.get('id') || '';
      if (this.exerciseId) {
        this.loadExercise();
      }
    });
  }

  loadExercise(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.exerciseService.getExerciseById(this.exerciseId).subscribe({
      next: (data) => {
        this.exercise = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load exercise details.';
      }
    });

    this.knowledgeService.getClaimsByExerciseId(this.exerciseId).subscribe({
      next: (claimsData) => {
        this.claims = claimsData;
      },
      error: () => {
        this.claims = [];
      }
    });
  }

  hasSecondaryMuscles(): boolean {
    return this.exercise?.muscles.some(m => !m.isPrimary) ?? false;
  }

  getRatingBadgeClass(rating: QualitativeRating): string {
    switch (rating) {
      case QualitativeRating.High:
        return 'rating-high';
      case QualitativeRating.Moderate:
        return 'rating-moderate';
      case QualitativeRating.Low:
        return 'rating-low';
      default:
        return '';
    }
  }
}
