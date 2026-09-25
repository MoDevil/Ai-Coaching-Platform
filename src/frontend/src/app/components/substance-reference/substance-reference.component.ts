import { Component, OnInit } from '@angular/core';
import { SubstanceService } from '../../services/substance.service';
import {
  SupplementKnowledgeSummary,
  SupplementKnowledge,
  HormoneKnowledgeSummary,
  HormoneKnowledge,
  PEDSafetyRecordSummary,
  PEDSafetyRecord,
  SubstanceSafetyEvaluationResult,
  SubstanceEscalationRecord,
  SubstanceEscalationLevel,
  SupplementCategory,
  HormoneAxis,
  PEDCategory,
  OrganSystem,
  PEDRiskSeverity
} from '../../models/substance.model';

@Component({
  selector: 'app-substance-reference',
  templateUrl: './substance-reference.component.html',
  styleUrls: ['./substance-reference.component.css']
})
export class SubstanceReferenceComponent implements OnInit {
  activeTab: 'supplements' | 'hormones' | 'pedSafety' | 'screener' = 'supplements';

  // Supplements
  supplements: SupplementKnowledgeSummary[] = [];
  selectedSupplement: SupplementKnowledge | null = null;
  loadingSupplements = false;

  // Hormones
  hormones: HormoneKnowledgeSummary[] = [];
  selectedHormone: HormoneKnowledge | null = null;
  loadingHormones = false;

  // PED Safety
  pedRecords: PEDSafetyRecordSummary[] = [];
  selectedPED: PEDSafetyRecord | null = null;
  loadingPEDs = false;

  // Safety Screener & Triage
  signalsInput = '';
  selectedSubstanceId = '';
  contextNotes = '';
  screeningInProgress = false;
  evaluationResult: SubstanceSafetyEvaluationResult | null = null;
  escalations: SubstanceEscalationRecord[] = [];
  loadingEscalations = false;

  errorMessage = '';

  constructor(private substanceService: SubstanceService) {}

  ngOnInit(): void {
    this.loadSupplements();
    this.loadHormones();
    this.loadPEDRecords();
    this.loadEscalations();
  }

  setTab(tab: 'supplements' | 'hormones' | 'pedSafety' | 'screener'): void {
    this.activeTab = tab;
    this.errorMessage = '';
  }

  loadSupplements(): void {
    this.loadingSupplements = true;
    this.substanceService.getSupplements().subscribe({
      next: (data) => {
        this.supplements = data;
        this.loadingSupplements = false;
        if (data.length > 0 && !this.selectedSupplement) {
          this.selectSupplement(data[0].id);
        }
      },
      error: (err) => {
        this.errorMessage = 'Failed to load supplements.';
        this.loadingSupplements = false;
      }
    });
  }

  selectSupplement(id: string): void {
    this.substanceService.getSupplementById(id).subscribe({
      next: (data) => {
        this.selectedSupplement = data;
      },
      error: () => {
        this.errorMessage = 'Failed to load supplement details.';
      }
    });
  }

  loadHormones(): void {
    this.loadingHormones = true;
    this.substanceService.getHormones().subscribe({
      next: (data) => {
        this.hormones = data;
        this.loadingHormones = false;
        if (data.length > 0 && !this.selectedHormone) {
          this.selectHormone(data[0].id);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load hormones.';
        this.loadingHormones = false;
      }
    });
  }

  selectHormone(id: string): void {
    this.substanceService.getHormoneById(id).subscribe({
      next: (data) => {
        this.selectedHormone = data;
      },
      error: () => {
        this.errorMessage = 'Failed to load hormone details.';
      }
    });
  }

  loadPEDRecords(): void {
    this.loadingPEDs = true;
    this.substanceService.getPEDSafetyRecords().subscribe({
      next: (data) => {
        this.pedRecords = data;
        this.loadingPEDs = false;
        if (data.length > 0 && !this.selectedPED) {
          this.selectPED(data[0].id);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load PED safety records.';
        this.loadingPEDs = false;
      }
    });
  }

  selectPED(id: string): void {
    this.substanceService.getPEDSafetyRecordById(id).subscribe({
      next: (data) => {
        this.selectedPED = data;
      },
      error: () => {
        this.errorMessage = 'Failed to load PED safety details.';
      }
    });
  }

  runScreening(): void {
    if (!this.signalsInput.trim()) {
      this.errorMessage = 'Please enter at least one symptom or signal to evaluate.';
      return;
    }

    const signals = this.signalsInput
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0);

    this.screeningInProgress = true;
    this.errorMessage = '';

    this.substanceService.evaluateSubstanceSafety({
      substanceRecordId: this.selectedSubstanceId ? this.selectedSubstanceId : undefined,
      reportedSignals: signals,
      contextNotes: this.contextNotes ? this.contextNotes : undefined
    }).subscribe({
      next: (result) => {
        this.evaluationResult = result;
        this.screeningInProgress = false;
        this.loadEscalations();
      },
      error: () => {
        this.errorMessage = 'Safety evaluation failed.';
        this.screeningInProgress = false;
      }
    });
  }

  loadEscalations(): void {
    this.loadingEscalations = true;
    this.substanceService.getCoachEscalations().subscribe({
      next: (data) => {
        this.escalations = data;
        this.loadingEscalations = false;
      },
      error: () => {
        this.loadingEscalations = false;
      }
    });
  }

  getEscalationBadgeClass(level: SubstanceEscalationLevel): string {
    switch (level) {
      case SubstanceEscalationLevel.EmergencyMedicalAttention:
        return 'badge-emergency';
      case SubstanceEscalationLevel.UrgentMedicalReferral:
        return 'badge-urgent';
      case SubstanceEscalationLevel.CautionCoachReview:
        return 'badge-caution';
      default:
        return 'badge-normal';
    }
  }

  getEscalationLabel(level: SubstanceEscalationLevel): string {
    switch (level) {
      case SubstanceEscalationLevel.EmergencyMedicalAttention:
        return 'Emergency Medical Attention (911/123)';
      case SubstanceEscalationLevel.UrgentMedicalReferral:
        return 'Urgent Medical Referral';
      case SubstanceEscalationLevel.CautionCoachReview:
        return 'Caution - Coach Review Required';
      default:
        return 'No Safety Concerns Detected';
    }
  }

  getEvidenceLevelLabel(level: number): string {
    switch (level) {
      case 5: return 'Meta-Analysis (Grade A)';
      case 6: return 'Clinical Guideline';
      case 4: return 'Randomized Controlled Trial';
      case 3: return 'Expert Consensus';
      case 2: return 'Mechanistic Rationale';
      default: return 'Evidence Level ' + level;
    }
  }

  getSupplementCategoryName(cat: SupplementCategory): string {
    return SupplementCategory[cat] || 'Supplement';
  }

  getHormoneAxisName(axis: HormoneAxis): string {
    return HormoneAxis[axis] || 'Endocrine Axis';
  }

  getPEDCategoryName(cat: PEDCategory): string {
    return PEDCategory[cat] || 'PED';
  }

  getOrganSystemName(org: OrganSystem): string {
    return OrganSystem[org] || 'Organ System';
  }

  getRiskSeverityName(sev: PEDRiskSeverity): string {
    return PEDRiskSeverity[sev] || 'Risk';
  }
}
