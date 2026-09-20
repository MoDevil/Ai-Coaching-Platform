import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProgramService } from '../../services/program.service';
import { AdaptationService } from '../../services/adaptation.service';
import {
  Program,
  ProgramStatus,
  ProgramStatusLabels,
  PrimaryGoalTypeLabels,
  RecoveryCapacityLabels,
  MusclePriorityLevelLabels,
  ProgressionRuleTypeLabels
} from '../../models/program.models';
import {
  AdaptationAssessment,
  AdaptationRecommendation,
  AdaptationOverallStatusLabels,
  AdaptationActionTypeLabels,
  RecommendationConfidenceLabels,
  RecommendationStatusLabels,
  RecommendationStatus,
  PerformanceTrendLabels,
  EffortAlignmentStatusLabels
} from '../../models/adaptation.models';

@Component({
  selector: 'app-program-detail',
  templateUrl: './program-detail.component.html',
  styleUrls: ['./program-detail.component.css']
})
export class ProgramDetailComponent implements OnInit {
  programId: string | null = null;
  clientId: string | null = null;
  program: Program | null = null;
  isLoading = true;
  errorMessage: string | null = null;
  activeWeekTab = 1;

  // M7 Adaptive Coaching State
  assessments: AdaptationAssessment[] = [];
  latestAssessment: AdaptationAssessment | null = null;
  isAssessing = false;
  adaptationFeedback: string | null = null;
  decisionNotes: Record<string, string> = {};

  readonly ProgramStatus = ProgramStatus;
  readonly ProgramStatusLabels = ProgramStatusLabels;
  readonly PrimaryGoalTypeLabels = PrimaryGoalTypeLabels;
  readonly RecoveryCapacityLabels = RecoveryCapacityLabels;
  readonly MusclePriorityLevelLabels = MusclePriorityLevelLabels;
  readonly ProgressionRuleTypeLabels = ProgressionRuleTypeLabels;

  readonly AdaptationOverallStatusLabels = AdaptationOverallStatusLabels;
  readonly AdaptationActionTypeLabels = AdaptationActionTypeLabels;
  readonly RecommendationConfidenceLabels = RecommendationConfidenceLabels;
  readonly RecommendationStatusLabels = RecommendationStatusLabels;
  readonly RecommendationStatus = RecommendationStatus;
  readonly PerformanceTrendLabels = PerformanceTrendLabels;
  readonly EffortAlignmentStatusLabels = EffortAlignmentStatusLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private programService: ProgramService,
    private adaptationService: AdaptationService
  ) {}

  ngOnInit(): void {
    this.programId = this.route.snapshot.paramMap.get('id');
    this.clientId = this.route.snapshot.queryParamMap.get('clientId');

    if (this.programId) {
      this.loadProgram(this.programId);
    } else if (this.clientId) {
      this.loadActiveProgramForClient(this.clientId);
    } else {
      this.errorMessage = 'No program or client identifier specified.';
      this.isLoading = false;
    }
  }

  loadProgram(id: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.programService.getProgramById(id).subscribe({
      next: (p) => {
        this.program = p;
        this.isLoading = false;
        if (p.activeVersion?.weeks.length) {
          this.activeWeekTab = p.activeVersion.weeks[0].weekNumber;
        }
        this.loadAssessments(p.id);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to load training program.';
        this.isLoading = false;
      }
    });
  }

  loadActiveProgramForClient(clientId: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.programService.getActiveProgramForClient(clientId).subscribe({
      next: (p) => {
        this.program = p;
        this.isLoading = false;
        if (p.activeVersion?.weeks.length) {
          this.activeWeekTab = p.activeVersion.weeks[0].weekNumber;
        }
        this.loadAssessments(p.id);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'No active program found for this client.';
        this.isLoading = false;
      }
    });
  }

  updateStatus(newStatus: ProgramStatus): void {
    if (!this.program) return;

    this.programService.updateProgramStatus(this.program.id, { status: newStatus }).subscribe({
      next: (updated) => {
        this.program = updated;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to update program status.';
      }
    });
  }

  loadAssessments(programId: string): void {
    this.adaptationService.getAssessmentsForProgram(programId).subscribe({
      next: (list) => {
        this.assessments = list;
        this.latestAssessment = list.length > 0 ? list[0] : null;
      },
      error: () => {
        // Non-blocking error for historical assessments
      }
    });
  }

  runAssessment(): void {
    if (!this.program?.activeVersion) return;
    this.isAssessing = true;
    this.adaptationFeedback = null;

    this.adaptationService.triggerAssessment(this.program.activeVersion.id).subscribe({
      next: (assessment) => {
        this.latestAssessment = assessment;
        this.assessments = [assessment, ...this.assessments];
        this.isAssessing = false;
        this.adaptationFeedback = 'Adaptive coaching evaluation completed successfully.';
      },
      error: (err) => {
        this.isAssessing = false;
        this.adaptationFeedback = err.error?.message || 'Failed to evaluate adaptive coaching recommendations.';
      }
    });
  }

  decideRecommendation(recommendation: AdaptationRecommendation, approve: boolean): void {
    const note = this.decisionNotes[recommendation.id] || (approve ? 'Approved by coach' : 'Rejected by coach');

    this.adaptationService.decideRecommendation(recommendation.id, {
      approve,
      coachDecisionNote: note
    }).subscribe({
      next: (updated) => {
        recommendation.status = updated.status;
        recommendation.coachDecisionNote = updated.coachDecisionNote;
        this.adaptationFeedback = approve
          ? 'Recommendation approved! New program version created and activated.'
          : 'Recommendation rejected.';

        if (approve && this.programId) {
          // Reload program to show new version
          this.loadProgram(this.programId);
        }
      },
      error: (err) => {
        this.adaptationFeedback = err.error?.message || 'Failed to submit recommendation decision.';
      }
    });
  }

  goBack(): void {
    if (this.program?.clientId) {
      this.router.navigate(['/clients', this.program.clientId]);
    } else {
      this.router.navigate(['/clients']);
    }
  }
}
