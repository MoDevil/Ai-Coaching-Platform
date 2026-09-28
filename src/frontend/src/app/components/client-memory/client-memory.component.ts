import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ClientMemoryRecordDto,
  ClientMemoryConflictDto,
  UnresolvedQuestionDto,
  ClientMemorySnapshotDto,
  MemoryCategory,
  MemoryConfidenceLevel,
  MemoryRecordStatus,
  ConflictResolutionAction,
  MemorySourceType
} from '../../models/memory.model';
import { MemoryService } from '../../services/memory.service';

@Component({
  selector: 'app-client-memory',
  templateUrl: './client-memory.component.html',
  styleUrls: ['./client-memory.component.css']
})
export class ClientMemoryComponent implements OnInit {
  clientId: string = '';
  memories: ClientMemoryRecordDto[] = [];
  conflicts: ClientMemoryConflictDto[] = [];
  unresolvedQuestions: UnresolvedQuestionDto[] = [];
  snapshot: ClientMemorySnapshotDto | null = null;
  isLoading = false;
  selectedCategory: number | null = null;
  includeAnonymized = false;

  memoryForm: FormGroup;
  correctionForm: FormGroup;
  selectedRecordForCorrection: ClientMemoryRecordDto | null = null;
  selectedConflictForResolution: ClientMemoryConflictDto | null = null;

  categories = [
    { value: MemoryCategory.Preference, label: 'Preference' },
    { value: MemoryCategory.Aversion, label: 'Aversion' },
    { value: MemoryCategory.PainObservation, label: 'Pain Observation' },
    { value: MemoryCategory.LifeEvent, label: 'Life Event' },
    { value: MemoryCategory.GoalContext, label: 'Goal Context' },
    { value: MemoryCategory.EquipmentConstraint, label: 'Equipment Constraint' },
    { value: MemoryCategory.ScheduleConstraint, label: 'Schedule Constraint' },
    { value: MemoryCategory.NutritionHabit, label: 'Nutrition Habit' },
    { value: MemoryCategory.AdherenceNote, label: 'Adherence Note' },
    { value: MemoryCategory.RecoveryNote, label: 'Recovery Note' },
    { value: MemoryCategory.GeneralNote, label: 'General Note' }
  ];

  constructor(
    private route: ActivatedRoute,
    private memoryService: MemoryService,
    private fb: FormBuilder
  ) {
    this.memoryForm = this.fb.group({
      memoryCategory: [MemoryCategory.Preference, Validators.required],
      content: ['', [Validators.required, Validators.maxLength(4000)]],
      sourceDescription: [''],
      sourceReference: ['']
    });

    this.correctionForm = this.fb.group({
      content: ['', [Validators.required, Validators.maxLength(4000)]],
      reason: ['', [Validators.required, Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    this.clientId = this.route.snapshot.paramMap.get('id') || '';
    if (this.clientId) {
      this.loadAll();
    }
  }

  loadAll(): void {
    this.loadMemories();
    this.loadConflicts();
    this.loadUnresolvedQuestions();
    this.loadSnapshot();
  }

  loadMemories(): void {
    this.isLoading = true;
    const cat = this.selectedCategory !== null ? (this.selectedCategory as MemoryCategory) : undefined;
    this.memoryService.getMemories(this.clientId, cat, this.includeAnonymized).subscribe({
      next: (data) => {
        this.memories = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadConflicts(): void {
    this.memoryService.getConflicts(this.clientId, true).subscribe({
      next: (data) => (this.conflicts = data)
    });
  }

  loadUnresolvedQuestions(): void {
    this.memoryService.getUnresolvedQuestions(this.clientId).subscribe({
      next: (data) => (this.unresolvedQuestions = data)
    });
  }

  loadSnapshot(): void {
    this.memoryService.getSnapshot(this.clientId).subscribe({
      next: (data) => (this.snapshot = data)
    });
  }

  onAddMemory(): void {
    if (this.memoryForm.invalid) return;

    this.memoryService.createMemory(this.clientId, {
      memoryCategory: Number(this.memoryForm.value.memoryCategory),
      content: this.memoryForm.value.content,
      sourceType: MemorySourceType.CoachRecorded,
      sourceDescription: this.memoryForm.value.sourceDescription,
      sourceReference: this.memoryForm.value.sourceReference
    }).subscribe({
      next: () => {
        this.memoryForm.reset({ memoryCategory: MemoryCategory.Preference });
        this.loadAll();
      }
    });
  }

  openCorrection(record: ClientMemoryRecordDto): void {
    this.selectedRecordForCorrection = record;
    this.correctionForm.patchValue({
      content: record.content,
      reason: ''
    });
  }

  onSaveCorrection(): void {
    if (!this.selectedRecordForCorrection || this.correctionForm.invalid) return;

    this.memoryService.correctMemory(this.clientId, this.selectedRecordForCorrection.id, {
      content: this.correctionForm.value.content,
      reason: this.correctionForm.value.reason
    }).subscribe({
      next: () => {
        this.selectedRecordForCorrection = null;
        this.correctionForm.reset();
        this.loadAll();
      }
    });
  }

  onFlagUncertain(record: ClientMemoryRecordDto): void {
    this.memoryService.flagUncertain(this.clientId, record.id).subscribe({
      next: () => this.loadAll()
    });
  }

  onArchive(record: ClientMemoryRecordDto): void {
    this.memoryService.archiveMemory(this.clientId, record.id).subscribe({
      next: () => this.loadAll()
    });
  }

  openConflictResolution(conflict: ClientMemoryConflictDto): void {
    this.selectedConflictForResolution = conflict;
  }

  onResolveConflict(action: ConflictResolutionAction, note: string): void {
    if (!this.selectedConflictForResolution) return;

    this.memoryService.resolveConflict(this.clientId, this.selectedConflictForResolution.id, {
      action,
      resolutionNote: note || 'Resolved by coach'
    }).subscribe({
      next: () => {
        this.selectedConflictForResolution = null;
        this.loadAll();
      }
    });
  }

  onGenerateSnapshot(): void {
    this.memoryService.generateSnapshot(this.clientId).subscribe({
      next: (snap) => (this.snapshot = snap)
    });
  }

  onAnonymize(): void {
    if (confirm('Are you sure you want to anonymize all memories for this client? Personal content will be permanently cleared.')) {
      this.memoryService.anonymizeMemories(this.clientId, 'Requested by coach').subscribe({
        next: () => this.loadAll()
      });
    }
  }

  getCategoryName(cat: MemoryCategory): string {
    const item = this.categories.find(c => c.value === cat);
    return item ? item.label : 'Note';
  }

  getConfidenceName(conf: MemoryConfidenceLevel): string {
    return MemoryConfidenceLevel[conf] || 'Unknown';
  }

  getStatusName(st: MemoryRecordStatus): string {
    return MemoryRecordStatus[st] || 'Unknown';
  }
}
