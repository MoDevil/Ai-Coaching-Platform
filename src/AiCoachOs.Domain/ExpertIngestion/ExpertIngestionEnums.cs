namespace AiCoachOs.Domain.ExpertIngestion;

public enum ExpertSourceType
{
    YouTubeChannel = 1,
    Podcast = 2,
    Blog = 3,
    ResearchGroup = 4,
    Other = 5
}

public enum IngestionSourceType
{
    YouTubeVideo = 1,
    Article = 2,
    PodcastEpisode = 3,
    Other = 4
}

public enum IngestionStatus
{
    Processing = 1,
    PendingReview = 2,
    PartiallyApproved = 3,
    Completed = 4,
    Failed = 5
}

public enum ClaimCategory
{
    TrainingVolume = 1,
    Frequency = 2,
    Intensity = 3,
    Nutrition = 4,
    Recovery = 5,
    Supplementation = 6,
    Biomechanics = 7,
    General = 8
}

public enum EvidenceClassification
{
    OpinionOnly = 1,
    InterpretationOfResearch = 2,
    CitesConcreteSources = 3,
    ContradictsCurrentEvidence = 4,
    AgreesWithCurrentEvidence = 5,
    Uncertain = 6
}

public enum CoachReviewStatus
{
    PendingReview = 1,
    Approved = 2,
    Rejected = 3,
    Deferred = 4
}

public enum CreatorConfidence
{
    Low = 1,
    Medium = 2,
    High = 3
}
