import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProgramService } from '../../services/program.service';
import {
  Program,
  ProgramStatus,
  ProgramStatusLabels,
  PrimaryGoalTypeLabels,
  RecoveryCapacityLabels,
  MusclePriorityLevelLabels,
  ProgressionRuleTypeLabels
} from '../../models/program.models';

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

  readonly ProgramStatus = ProgramStatus;
  readonly ProgramStatusLabels = ProgramStatusLabels;
  readonly PrimaryGoalTypeLabels = PrimaryGoalTypeLabels;
  readonly RecoveryCapacityLabels = RecoveryCapacityLabels;
  readonly MusclePriorityLevelLabels = MusclePriorityLevelLabels;
  readonly ProgressionRuleTypeLabels = ProgressionRuleTypeLabels;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private programService: ProgramService
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

  goBack(): void {
    if (this.program?.clientId) {
      this.router.navigate(['/clients', this.program.clientId]);
    } else {
      this.router.navigate(['/clients']);
    }
  }
}
