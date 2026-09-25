using AiCoachOs.Domain.Knowledge;

namespace AiCoachOs.Domain.Substances;

/// <summary>
/// Synthetic test fixtures and seed data definitions for unit and integration tests.
/// </summary>
public static class SubstanceSeedData
{
    // Knowledge Sources IDs
    public static readonly Guid IssnCreatineSourceId = Guid.Parse("33333333-0000-0000-0000-000000000001");
    public static readonly Guid IssnCaffeineSourceId = Guid.Parse("33333333-0000-0000-0000-000000000002");
    public static readonly Guid ProteinMetaAnalysisSourceId = Guid.Parse("33333333-0000-0000-0000-000000000003");
    public static readonly Guid EndocrineSocietyTestosteroneSourceId = Guid.Parse("33333333-0000-0000-0000-000000000004");
    public static readonly Guid EndocrineCortisolSourceId = Guid.Parse("33333333-0000-0000-0000-000000000005");
    public static readonly Guid WadaPedSafetySourceId = Guid.Parse("33333333-0000-0000-0000-000000000006");

    // Knowledge Claims IDs
    public static readonly Guid CreatineClaimId = Guid.Parse("44444444-0000-0000-0000-000000000001");
    public static readonly Guid CaffeineClaimId = Guid.Parse("44444444-0000-0000-0000-000000000002");
    public static readonly Guid WheyProteinClaimId = Guid.Parse("44444444-0000-0000-0000-000000000003");
    public static readonly Guid TestosteroneClaimId = Guid.Parse("44444444-0000-0000-0000-000000000004");
    public static readonly Guid CortisolClaimId = Guid.Parse("44444444-0000-0000-0000-000000000005");
    public static readonly Guid AasCardioRiskClaimId = Guid.Parse("44444444-0000-0000-0000-000000000006");
    public static readonly Guid SarmSafetyClaimId = Guid.Parse("44444444-0000-0000-0000-000000000007");

    // Substance IDs
    public static readonly Guid CreatineId = Guid.Parse("55555555-0000-0000-0000-000000000001");
    public static readonly Guid CaffeineId = Guid.Parse("55555555-0000-0000-0000-000000000002");
    public static readonly Guid WheyProteinId = Guid.Parse("55555555-0000-0000-0000-000000000003");
    public static readonly Guid TestosteroneId = Guid.Parse("55555555-0000-0000-0000-000000000004");
    public static readonly Guid CortisolId = Guid.Parse("55555555-0000-0000-0000-000000000005");
    public static readonly Guid ThyroidId = Guid.Parse("55555555-0000-0000-0000-000000000006");
    public static readonly Guid AasOverviewId = Guid.Parse("55555555-0000-0000-0000-000000000007");
    public static readonly Guid SarmOverviewId = Guid.Parse("55555555-0000-0000-0000-000000000008");

    // Red Flag Rules IDs
    public static readonly Guid RulePedCardioEmergencyId = Guid.Parse("66666666-0000-0000-0000-000000000001");
    public static readonly Guid RulePedHepaticJaundiceId = Guid.Parse("66666666-0000-0000-0000-000000000002");
    public static readonly Guid RulePedHypertensiveCrisisId = Guid.Parse("66666666-0000-0000-0000-000000000003");
    public static readonly Guid RulePedPsychiatricEmergencyId = Guid.Parse("66666666-0000-0000-0000-000000000004");
    public static readonly Guid RulePedEndocrineSuppressionId = Guid.Parse("66666666-0000-0000-0000-000000000005");

    public static IReadOnlyList<KnowledgeSource> GetKnowledgeSources()
    {
        return new List<KnowledgeSource>
        {
            new KnowledgeSource(
                id: IssnCreatineSourceId,
                sourceType: KnowledgeSourceType.PositionStand,
                title: "International Society of Sports Nutrition position stand: safety and efficacy of creatine supplementation in exercise, sport, and medicine",
                authors: "Kreider RB, Kalman DS, Antonio J, et al.",
                year: 2017,
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                doi: "10.1186/s12970-017-0173-z",
                url: "https://doi.org/10.1186/s12970-017-0173-z",
                notes: "Journal of the International Society of Sports Nutrition, 14(1):18, 2017."),

            new KnowledgeSource(
                id: IssnCaffeineSourceId,
                sourceType: KnowledgeSourceType.PositionStand,
                title: "International society of sports nutrition position stand: coffee and caffeine ergogenic effects",
                authors: "Guest NS, VanDusseldorp TA, Nelson MT, et al.",
                year: 2021,
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                doi: "10.1186/s12970-020-00383-4",
                url: "https://doi.org/10.1186/s12970-020-00383-4",
                notes: "Journal of the International Society of Sports Nutrition, 18(1):1, 2021."),

            new KnowledgeSource(
                id: ProteinMetaAnalysisSourceId,
                sourceType: KnowledgeSourceType.ScientificPaper,
                title: "A systematic review, meta-analysis and meta-regression of the effect of protein supplementation on resistance training-induced gains in muscle mass and strength in healthy adults",
                authors: "Morton RW, Murphy KT, McKellar SR, et al.",
                year: 2018,
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                doi: "10.1136/bjsports-2017-097608",
                url: "https://doi.org/10.1136/bjsports-2017-097608",
                notes: "British Journal of Sports Medicine, 52(6):376-384, 2018."),

            new KnowledgeSource(
                id: EndocrineSocietyTestosteroneSourceId,
                sourceType: KnowledgeSourceType.PositionStand,
                title: "Testosterone Therapy in Men With Hypogonadism: An Endocrine Society Clinical Practice Guideline",
                authors: "Bhasin S, Brito JP, Cunningham GR, et al.",
                year: 2018,
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                doi: "10.1210/jc.2018-00229",
                url: "https://doi.org/10.1210/jc.2018-00229",
                notes: "The Journal of Clinical Endocrinology & Metabolism, 103(5):1715-1744, 2018."),

            new KnowledgeSource(
                id: EndocrineCortisolSourceId,
                sourceType: KnowledgeSourceType.PositionStand,
                title: "Diagnosis and Complications of Cushing's Syndrome & Hypothalamic-Pituitary-Adrenal Axis Regulation",
                authors: "Nieman LK, Biller BM, Findling JW, et al.",
                year: 2015,
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                doi: "10.1210/jc.2015-1818",
                url: "https://doi.org/10.1210/jc.2015-1818",
                notes: "The Journal of Clinical Endocrinology & Metabolism, 100(8):2807-2831, 2015."),

            new KnowledgeSource(
                id: WadaPedSafetySourceId,
                sourceType: KnowledgeSourceType.ScientificPaper,
                title: "Adverse Health Effects of Anabolic Androgenic Steroids and Novel Selective Androgen Receptor Modulators",
                authors: "Pope HG Jr, Wood RI, Rogol A, et al.",
                year: 2014,
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                doi: "10.1210/er.2013-1058",
                url: "https://doi.org/10.1210/er.2013-1058",
                notes: "Endocrine Reviews, 35(3):341-375, 2014.")
        };
    }

    public static IReadOnlyList<KnowledgeClaim> GetKnowledgeClaims()
    {
        var claims = new List<KnowledgeClaim>
        {
            new KnowledgeClaim(
                id: CreatineClaimId,
                topic: "Supplements - Creatine Monohydrate",
                question: "Does creatine monohydrate supplementation enhance strength, power, and lean mass accretion in resistance-trained individuals?",
                claimText: "Creatine monohydrate is the most extensively studied and effective ergogenic nutritional supplement currently available for increasing high-intensity exercise capacity and lean body mass during resistance training.",
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                status: ClaimStatus.Active,
                population: "Healthy exercising adults and athletes",
                limitations: "Individual responsiveness varies (approx. 20-30% non-responders based on baseline intramuscular phosphocreatine stores). Minor transient water retention upon loading.",
                practicalApplication: "Standard maintenance dose of 3-5 g/day or loading protocol of 20 g/day for 5-7 days followed by maintenance.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board"),

            new KnowledgeClaim(
                id: CaffeineClaimId,
                topic: "Supplements - Caffeine",
                question: "Does acute caffeine intake improve muscular endurance, power output, and cognitive alertness in resistance training?",
                claimText: "Caffeine reliably enhances muscular endurance, movement velocity, maximal strength, and perceived exertion attenuation when ingested 30-60 minutes prior to exercise.",
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                status: ClaimStatus.Active,
                population: "Adult resistance trainees",
                limitations: "Tolerance develops with habitual use. May impair sleep latency and sleep quality if consumed within 6-8 hours of bedtime. High doses (>6 mg/kg) increase anxiety and jitteriness without further ergogenic benefit.",
                practicalApplication: "Doses of 3-6 mg/kg body weight ingested approximately 45-60 min pre-workout. Modulate for evening sessions.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board"),

            new KnowledgeClaim(
                id: WheyProteinClaimId,
                topic: "Supplements - Whey Protein",
                question: "Does supplemental whey protein facilitate muscle protein synthesis and recovery when whole-food protein is insufficient?",
                claimText: "Whey protein supplementation provides a high biological value source of essential amino acids and leucine, effectively stimulating muscle protein synthesis when total daily protein targets are unmet through whole foods.",
                evidenceLevel: EvidenceLevel.MetaAnalysis,
                status: ClaimStatus.Active,
                population: "Resistance-trained individuals",
                limitations: "Supplements offer no inherent anabolic advantage over equivalent high-quality whole food protein when total daily protein is equated.",
                practicalApplication: "Convenient post-workout or meal-supplementing source of 20-40 g high-quality protein.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board"),

            new KnowledgeClaim(
                id: TestosteroneClaimId,
                topic: "Endocrine - Testosterone & HPTA Axis",
                question: "How does the hypothalamic-pituitary-testicular axis regulate endogenous testosterone and respond to training stimulus?",
                claimText: "Endogenous testosterone regulates muscle protein accretion, bone mineral density, and erythropoiesis through androgen receptor signaling. Heavy resistance exercise produces transient acute elevations without permanently shifting basal endocrine setpoints, whereas chronic energy deficiency or overreaching suppresses the HPTA axis.",
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                status: ClaimStatus.Active,
                population: "Adult males and females (axis dynamics)",
                limitations: "Circulating total testosterone exhibits circadian variation (peaking in early morning) and requires multi-point morning testing for clinical diagnostic accuracy.",
                practicalApplication: "Coaches must prioritize adequate energy availability, sleep hygiene, and progressive volume management to avoid endogenous endocrine suppression.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board"),

            new KnowledgeClaim(
                id: CortisolClaimId,
                topic: "Endocrine - Cortisol & Stress Adaptation",
                question: "What is the physiological role of cortisol during exercise and how does chronic elevation affect recovery?",
                claimText: "Cortisol is a glucocorticoid hormone critical for substrate mobilization and anti-inflammatory signaling during acute exercise. Chronic non-functional elevation due to sustained excessive stress, sleep deprivation, or severe caloric deficit impairs muscular recovery, immune function, and sleep architecture.",
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                status: ClaimStatus.Active,
                population: "Active exercising adults",
                limitations: "Acute post-workout spikes are a normal physiological adaptation; chronic resting elevation indicates systemic recovery deficit.",
                practicalApplication: "Monitor systemic fatigue cues (sleep disturbances, persistent resting soreness) to prevent maladaptive stress accumulation.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board"),

            new KnowledgeClaim(
                id: AasCardioRiskClaimId,
                topic: "PED Safety - Cardiovascular and Endocrine Pathology",
                question: "What are the primary documented organ-system health risks associated with supraphysiological anabolic steroid use?",
                claimText: "Exogenous supraphysiological anabolic steroid exposure induces profound HPTA suppression, left ventricular hypertrophy, accelerated atherosclerosis via atherogenic dyslipidemia (profound HDL suppression and LDL elevation), endothelial dysfunction, hypertension, and hepatic stress with oral alkylated compounds.",
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                status: ClaimStatus.Active,
                population: "Individuals exposed to exogenous androgens",
                limitations: "Risk magnitude escalates with dosage, duration, compound polypharmacy, and genetic predisposition; complete reversibility of cardiac remodeling is not guaranteed.",
                practicalApplication: "Coaches must recognize adverse cardiovascular symptoms as emergency red flags requiring immediate medical evaluation.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Safety & Medical Board"),

            new KnowledgeClaim(
                id: SarmSafetyClaimId,
                topic: "PED Safety - SARMs Unapproved Substance Risks",
                question: "What safety concerns and endocrine effects are associated with unapproved Selective Androgen Receptor Modulators (SARMs)?",
                claimText: "SARMs are unapproved investigational compounds that reliably suppress endogenous gonadotropins and testosterone in a dose-dependent manner, while presenting risks of hepatotoxicity (elevated transaminases, drug-induced liver injury) and cardiovascular lipid derangement.",
                evidenceLevel: EvidenceLevel.ClinicalGuideline,
                status: ClaimStatus.Active,
                population: "Individuals exposed to unapproved selective androgen receptor modulators",
                limitations: "Long-term human safety trials are absent; over-the-counter and online products frequently contain unverified doses, adulterants, or mislabeled compounds.",
                practicalApplication: "Coaches must inform trainees of lack of human safety approval, genuine endocrine suppression, and potential hepatotoxicity.",
                reviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Safety & Medical Board")
        };

        // Link sources
        claims[0].AddSource(IssnCreatineSourceId, "Primary position stand on safety, dosing, and efficacy");
        claims[1].AddSource(IssnCaffeineSourceId, "Primary position stand on caffeine ergogenic effects");
        claims[2].AddSource(ProteinMetaAnalysisSourceId, "Meta-analysis on protein supplementation and resistance training gains");
        claims[3].AddSource(EndocrineSocietyTestosteroneSourceId, "Clinical guidelines on testosterone regulation and HPTA axis");
        claims[4].AddSource(EndocrineCortisolSourceId, "Clinical references on HPA axis and cortisol dynamics");
        claims[5].AddSource(WadaPedSafetySourceId, "Endocrine reviews on adverse health consequences of AAS");
        claims[6].AddSource(WadaPedSafetySourceId, "Safety analysis on SARMs and experimental androgenic compounds");

        return claims;
    }

    public static IReadOnlyList<SupplementKnowledge> GetSupplements()
    {
        var creatine = new SupplementKnowledge(
            id: CreatineId,
            name: "Creatine Monohydrate",
            primaryClaimedBenefit: "Increases intracellular phosphocreatine stores, accelerating ATP resynthesis during short-duration, high-intensity muscular contractions.",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "High-grade ergogenic compound that increases intracellular phosphocreatine stores, accelerating ATP resynthesis during short-duration, high-intensity muscular contractions.",
            uncertaintyStatement: "Efficacy is modulated by baseline muscle phosphocreatine concentration. Individuals with already saturated intramuscular stores (frequent red meat consumers) may experience diminished relative gains. Mild initial water weight gain is normal and non-pathological.",
            efficacyClaim: "Supported by hundreds of randomized controlled trials and meta-analyses demonstrating statistically significant increases in maximal strength (5-15%), repetitive sprint performance, and lean mass accretion.",
            populationNote: "Healthy exercising adults and athletes",
            typicalDoseRangeMin: 3m,
            typicalDoseRangeMax: 5m,
            doseUnit: "g/day",
            timingNote: "Consistent daily intake; timing relative to workout is secondary to chronic saturation",
            commonForms: "Creatine Monohydrate (Creapure preferred standard), Micronized Creatine",
            interactionsAndNotes: "Safe for healthy adults. Does not cause renal dysfunction in individuals with healthy kidneys. Adequate daily hydration recommended.",
            isEgyptianMarketAvailable: true,
            primaryKnowledgeClaimId: CreatineClaimId,
            lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            reviewedBy: "M12 Scientific Review Board",
            commonAliases: new[] { "Creatine", "Creapure" });

        creatine.AddSafetyFlag(new SubstanceSafetyFlag(
            category: SafetyFlagCategory.SpecialPopulationPrecaution,
            description: "Individuals with pre-existing severe renal disease should consult their nephrologist before initiating high-dose creatine supplementation.",
            escalationLevel: EscalationLevel.CoachAwareness,
            coachNote: "Standard clinical nephrology precaution regarding exogenous creatinine metabolite load."));

        var caffeine = new SupplementKnowledge(
            id: CaffeineId,
            name: "Caffeine",
            primaryClaimedBenefit: "Central nervous system stimulant and adenosine receptor antagonist that reduces perception of effort, increases motor unit recruitment, and enhances muscular endurance.",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Central nervous system stimulant and adenosine receptor antagonist that reduces perception of effort, increases motor unit recruitment, and enhances muscular endurance.",
            uncertaintyStatement: "High inter-individual variation due to CYP1A2 genotype (fast vs. slow metabolizers). Habituation reduces subjective alertness effects while retaining partial neuromuscular benefit. Evening ingestion impairs deep sleep architecture.",
            efficacyClaim: "Meta-analytic evidence shows robust ergogenic effects across maximal strength, power velocity, and muscular endurance at doses of 3-6 mg/kg body weight.",
            populationNote: "Adult resistance trainees",
            typicalDoseRangeMin: 3m,
            typicalDoseRangeMax: 6m,
            doseUnit: "mg/kg",
            timingNote: "45-60 minutes prior to resistance training session. Avoid within 6-8 hours of sleep.",
            commonForms: "Caffeine Anhydrous, Coffee, Pre-workout formulations",
            interactionsAndNotes: "Doses exceeding 6-9 mg/kg increase incidence of tremors, tachycardia, gastrointestinal distress, and anxiety without additional performance benefits.",
            isEgyptianMarketAvailable: true,
            primaryKnowledgeClaimId: CaffeineClaimId,
            lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            reviewedBy: "M12 Scientific Review Board",
            commonAliases: new[] { "1,3,7-trimethylxanthine", "Coffee Extract" });

        caffeine.AddSafetyFlag(new SubstanceSafetyFlag(
            category: SafetyFlagCategory.Contraindication,
            description: "High stimulant doses in sensitive individuals or those with uncontrolled hypertension may provoke palpitations or severe sleep degradation.",
            escalationLevel: EscalationLevel.CoachAwareness,
            coachNote: "ISSN Caffeine Position Stand 2021."));

        var whey = new SupplementKnowledge(
            id: WheyProteinId,
            name: "Whey Protein",
            primaryClaimedBenefit: "Rapidly digesting, high-biological-value milk protein concentrate or isolate rich in branched-chain amino acids for triggering muscle protein synthesis.",
            evidenceStatus: SupplementEvidenceStatus.StrongEvidence,
            effectMagnitude: EffectMagnitude.Moderate,
            description: "Rapidly digesting, high-biological-value milk protein concentrate or isolate rich in branched-chain amino acids (particularly leucine) for triggering muscle protein synthesis.",
            uncertaintyStatement: "Provides no unique metabolic advantage over whole-food protein when daily intake is 1.6-2.2 g/kg. Serves purely as a convenient, high-efficiency dietary protein vehicle.",
            efficacyClaim: "Meta-analyses demonstrate that when total daily protein is equated, whey is equivalent to other high-quality whole food protein sources in supporting hypertrophy and strength.",
            populationNote: "Resistance-trained individuals",
            typicalDoseRangeMin: 20m,
            typicalDoseRangeMax: 40m,
            doseUnit: "g/serving",
            timingNote: "Flexible; effective post-workout or distributed between meals to hit daily protein targets",
            commonForms: "Whey Protein Concentrate (80%), Whey Protein Isolate (90%), Whey Hydrolysate",
            interactionsAndNotes: "Individuals with lactose intolerance should favor Whey Isolate or plant-based alternatives. Not a meal replacement.",
            isEgyptianMarketAvailable: true,
            primaryKnowledgeClaimId: WheyProteinClaimId,
            lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            reviewedBy: "M12 Scientific Review Board",
            commonAliases: new[] { "WPC", "WPI", "Whey Isolate" });

        return new List<SupplementKnowledge> { creatine, caffeine, whey };
    }

    public static IReadOnlyList<HormoneKnowledge> GetHormones()
    {
        return new List<HormoneKnowledge>
        {
            new HormoneKnowledge(
                id: TestosteroneId,
                name: "Testosterone (Endocrine Axis)",
                hormoneCategory: HormoneCategory.Androgen,
                description: "Primary anabolic steroid hormone in humans, synthesized by Leydig cells in the testes (males) and the ovaries/adrenal cortex (females) under the control of LH and GnRH.",
                physiologicalRole: "Stimulates muscle protein synthesis, satellite cell activation, bone density preservation, neural drive, and erythropoiesis via intracellular androgen receptor signaling.",
                trainingRelevance: "Heavy resistance training elicits transient post-exercise surges. Long-term training adaptations depend on chronic baseline receptor sensitivity and systemic endocrine homeostasis rather than short-lived acute spikes.",
                uncertaintyStatement: "Serum total and free testosterone vary significantly throughout the day (morning peak) and are influenced by acute sleep, nutritional status, and body composition. Diagnostic evaluation requires strict morning laboratory assessment.",
                primaryKnowledgeClaimId: TestosteroneClaimId,
                biomarkerReferenceNotes: "Typical clinical morning total testosterone reference ranges: Adult Males ~300-1000 ng/dL (10.4-34.7 nmol/L); Adult Females ~15-70 ng/dL (0.5-2.4 nmol/L).",
                lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board",
                commonAliases: new[] { "Test", "Androgen" }),

            new HormoneKnowledge(
                id: CortisolId,
                name: "Cortisol (Adrenal Axis)",
                hormoneCategory: HormoneCategory.Glucocorticoid,
                description: "Primary glucocorticoid hormone produced by the adrenal cortex in response to ACTH from the pituitary gland, acting as a master regulator of stress response and substrate mobilization.",
                physiologicalRole: "Stimulates gluconeogenesis, lipolysis, and temporary anti-inflammatory cascades during acute physiological stress. Promotes muscle protein catabolism under sustained high concentrations.",
                trainingRelevance: "Acute elevations during heavy sessions are normal and necessary for energy mobilization. Chronic basal elevation from insufficient sleep or extreme training volume signals overreaching and recovery failure.",
                uncertaintyStatement: "Single random cortisol blood tests possess high noise due to acute situational stress and diurnal rhythm. Salivary 4-point cortisol or 24-hour urinary cortisol provide superior clinical assessment.",
                primaryKnowledgeClaimId: CortisolClaimId,
                biomarkerReferenceNotes: "Normal diurnal pattern: High upon waking (cortisol awakening response), progressively declining to lowest levels at midnight.",
                lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board",
                commonAliases: new[] { "Hydrocortisone", "Stress Hormone" }),

            new HormoneKnowledge(
                id: ThyroidId,
                name: "Thyroid Hormones (T3 / T4 Axis)",
                hormoneCategory: HormoneCategory.Thyroid,
                description: "Thyroxine (T4) and Triiodothyronine (T3) produced by the thyroid gland under TSH control, governing basal metabolic rate, protein synthesis, and mitochondrial respiration.",
                physiologicalRole: "Controls cellular oxygen consumption, thermogenesis, lipid oxidation, and resting energy expenditure throughout the body.",
                trainingRelevance: "Prolonged severe caloric restriction or low energy availability downregulates deiodinase activity, converting T4 to inactive reverse T3 (rT3), slowing metabolic rate.",
                uncertaintyStatement: "Metabolic rate slowing during cutting phases is partially reversible with adequate refeeding and energy restoration. Blood TSH and Free T3/T4 must be evaluated by an endocrinologist if chronic lethargy persists.",
                primaryKnowledgeClaimId: null,
                biomarkerReferenceNotes: "TSH normal range typically 0.4-4.0 mIU/L; Free T3 ~2.3-4.2 pg/mL; Free T4 ~0.8-1.8 ng/dL.",
                lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                reviewedBy: "M12 Scientific Review Board",
                commonAliases: new[] { "T3", "T4", "Thyroxine" })
        };
    }

    public static IReadOnlyList<PEDSafetyRecord> GetPEDSafetyRecords()
    {
        var aas = new PEDSafetyRecord(
            id: AasOverviewId,
            name: "Anabolic-Androgenic Steroids (AAS) Overview",
            pedCategory: PEDCategory.AAS,
            description: "Synthetic derivatives of testosterone designed to maximize anabolic (tissue-building) effects while possessing inherent androgenic (masculinizing) properties.",
            mechanismSummary: "Binds to intracellular androgen receptors, translocating to the nucleus to upregulate transcription of structural contractile proteins, enhancing nitrogen retention and satellite cell proliferation.",
            primaryKnowledgeClaimId: AasCardioRiskClaimId,
            monitoringConcepts: new[] { "Lipid Panel (HDL/LDL)", "Complete Blood Count (Hematocrit)", "Echocardiogram", "Liver Panel (AST/ALT/Bilirubin)" },
            lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            reviewedBy: "M12 Safety & Medical Board",
            commonAliases: new[] { "AAS", "Anabolic Steroids" });

        aas.AddRisk(new PEDRiskRecord(
            id: Guid.Parse("77777777-0000-0000-0000-000000000001"),
            pedSafetyRecordId: AasOverviewId,
            riskCategory: RiskCategory.Cardiovascular,
            severity: PEDRiskSeverity.Critical,
            description: "Left ventricular concentric hypertrophy, arterial stiffness, coronary atherosclerosis acceleration, severe HDL reduction, and elevated risk of thrombotic events / sudden cardiac death.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Lipid alterations may normalize upon cessation; structural left ventricular fibrosis and vascular remodeling are frequently irreversible.",
            evidenceClaimId: AasCardioRiskClaimId));

        aas.AddRisk(new PEDRiskRecord(
            id: Guid.Parse("77777777-0000-0000-0000-000000000002"),
            pedSafetyRecordId: AasOverviewId,
            riskCategory: RiskCategory.Endocrine,
            severity: PEDRiskSeverity.High,
            description: "Profound suppression/shutdown of GnRH, LH, and FSH secretion resulting in severe testicular atrophy, azoospermia, and secondary hypogonadism upon cessation.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Recovery of endogenous HPTA axis varies widely from months to years; permanent hypogonadism occurs in substantial subset of chronic users."));

        aas.AddRisk(new PEDRiskRecord(
            id: Guid.Parse("77777777-0000-0000-0000-000000000003"),
            pedSafetyRecordId: AasOverviewId,
            riskCategory: RiskCategory.Hepatic,
            severity: PEDRiskSeverity.High,
            description: "Oral 17-alpha-alkylated compounds cause cholestasis, transaminase elevation, peliosis hepatis, and increased risk of hepatic adenomas.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Transaminases usually normalize within weeks of cessation, but severe cholestatic jaundice requires immediate clinical medical care."));

        var sarm = new PEDSafetyRecord(
            id: SarmOverviewId,
            name: "Selective Androgen Receptor Modulators (SARMs) Overview",
            pedCategory: PEDCategory.SARM,
            description: "Non-steroidal ligands designed to selectively bind androgen receptors in skeletal muscle and bone while minimizing prostatic and sebaceous stimulation.",
            mechanismSummary: "Tissue-selective androgen receptor agonism without 5-alpha reduction to dihydrotestosterone (DHT) or aromatization to estradiol.",
            primaryKnowledgeClaimId: SarmSafetyClaimId,
            monitoringConcepts: new[] { "Liver Enzymes (ALT/AST)", "Lipid Panel", "Total & Free Testosterone" },
            lastReviewedAtUtc: new DateTime(2026, 9, 24, 0, 0, 0, DateTimeKind.Utc),
            reviewedBy: "M12 Safety & Medical Board",
            commonAliases: new[] { "SARM", "Investigational Androgens" });

        sarm.AddRisk(new PEDRiskRecord(
            id: Guid.Parse("77777777-0000-0000-0000-000000000004"),
            pedSafetyRecordId: SarmOverviewId,
            riskCategory: RiskCategory.Hepatic,
            severity: PEDRiskSeverity.High,
            description: "Drug-induced liver injury, elevated ALT/AST, cholestatic jaundice, and hepatic inflammation.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Usually reversible upon discontinuation, but clinical liver monitoring is essential if symptoms arise.",
            evidenceClaimId: SarmSafetyClaimId));

        sarm.AddRisk(new PEDRiskRecord(
            id: Guid.Parse("77777777-0000-0000-0000-000000000005"),
            pedSafetyRecordId: SarmOverviewId,
            riskCategory: RiskCategory.Endocrine,
            severity: PEDRiskSeverity.Moderate,
            description: "Dose-dependent suppression of endogenous testosterone and SHBG, leading to lethargy, libido loss, and post-cycle endocrine deficit.",
            evidenceLevel: EvidenceLevel.ClinicalGuideline,
            reversibilityNotes: "Endogenous axis typically recovers gradually, but may take several weeks to months."));

        return new List<PEDSafetyRecord> { aas, sarm };
    }

    public static IReadOnlyList<PEDRedFlagRule> GetPEDRedFlagRules()
    {
        return new List<PEDRedFlagRule>
        {
            // 1. Cardiovascular Emergency in PED context -> UrgentMedicalAttention
            new PEDRedFlagRule(
                id: RulePedCardioEmergencyId,
                name: "PED Cardiovascular Emergency Symptoms",
                description: "Chest pressure, radiating pain to left arm or jaw, severe acute palpitations, sudden resting dyspnea, or syncope in an individual with suspected or confirmed PED exposure.",
                signalPattern: "PED_Emergency_Cardiovascular",
                escalationLevel: EscalationLevel.UrgentMedicalAttention,
                recommendedAction: "IMMEDIATE EMERGENCY MEDICAL ATTENTION REQUIRED: Cease all physical activity immediately and call emergency medical services (e.g. 123 in Egypt). Do not allow client to drive or continue training.",
                evidenceBasis: "Standard emergency cardiology triage for acute coronary syndrome and malignant arrhythmia in androgen/stimulant users.",
                pedCategory: PEDCategory.AAS,
                sourceClaimId: AasCardioRiskClaimId),

            // 2. Acute Hypertensive Crisis -> UrgentMedicalAttention
            new PEDRedFlagRule(
                id: RulePedHypertensiveCrisisId,
                name: "Hypertensive Crisis & Neurological Symptoms",
                description: "Severe acute occipital headache, blurred vision, dizziness, spontaneous epistaxis (nosebleeds), or acute confusion alongside elevated blood pressure.",
                signalPattern: "PED_Hypertensive_Crisis",
                escalationLevel: EscalationLevel.UrgentMedicalAttention,
                recommendedAction: "EMERGENCY EVALUATION: High risk of hypertensive encephalopathy or vascular event. Halt workout immediately and seek urgent emergency room evaluation.",
                evidenceBasis: "AHA and clinical hypertension guidelines for hypertensive emergency screening.",
                pedCategory: PEDCategory.AAS,
                sourceClaimId: AasCardioRiskClaimId),

            // 3. Hepatic Dysfunction & Cholestasis -> HealthcareProfessionalReferral
            new PEDRedFlagRule(
                id: RulePedHepaticJaundiceId,
                name: "Hepatic Toxicity & Cholestatic Jaundice",
                description: "Yellowing of eyes/skin (jaundice/scleral icterus), dark tea-colored urine, pale stools, severe right upper quadrant abdominal tenderness, or persistent unexplained nausea.",
                signalPattern: "PED_Hepatic_Jaundice",
                escalationLevel: EscalationLevel.HealthcareProfessionalReferral,
                recommendedAction: "URGENT MEDICAL REFERRAL: Prompt clinical referral to a gastroenterologist or hepatologist for comprehensive hepatic panel (AST/ALT/Bilirubin/ALP). Cease all unprescribed compounds.",
                evidenceBasis: "Clinical guidelines on drug-induced liver injury (DILI) and androgenic hepatotoxicity.",
                pedCategory: PEDCategory.SARM,
                sourceClaimId: SarmSafetyClaimId),

            // 4. Acute Severe Psychiatric Symptoms -> HealthcareProfessionalReferral
            new PEDRedFlagRule(
                id: RulePedPsychiatricEmergencyId,
                name: "Acute Neuropsychiatric & Mood Disturbance",
                description: "Severe acute paranoia, hallucinations, unprovoked aggressive outbursts (rage), profound mania, or acute suicidal ideation associated with compound exposure or abrupt withdrawal.",
                signalPattern: "PED_Psychiatric_Emergency",
                escalationLevel: EscalationLevel.HealthcareProfessionalReferral,
                recommendedAction: "URGENT PSYCHIATRIC / MEDICAL REFERRAL: Immediate referral to qualified mental health professional or emergency crisis team. Ensure client safety.",
                evidenceBasis: "Psychiatric consensus on anabolic steroid-induced hypomania, depression, and affective disturbance."),

            // 5. Severe Endocrine Suppression / Hypogonadism -> CoachAwareness
            new PEDRedFlagRule(
                id: RulePedEndocrineSuppressionId,
                name: "Severe Endocrine Axis Suppression",
                description: "Severe testicular atrophy, persistent erectile dysfunction, profound lethargy, and severe depressive mood following compound cessation.",
                signalPattern: "PED_Endocrine_Suppression",
                escalationLevel: EscalationLevel.CoachAwareness,
                recommendedAction: "COACH REVIEW & MEDICAL REFERRAL: Refer client to an endocrinologist for comprehensive hormone panel evaluation. Adjust training volume conservatively to avoid overtraining during endocrine recovery.",
                evidenceBasis: "Endocrine Society clinical guidelines on secondary hypogonadism management.")
        };
    }
}
