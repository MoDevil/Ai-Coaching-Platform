namespace AiCoachOs.Domain.ExpertIngestion;

public enum IngestionContentType
{
    YouTube = 1,
    Article = 2,
    Podcast = 3
}

public enum IngestionStatus
{
    Processing = 1,
    PendingReview = 2,
    PartiallyApproved = 3,
    FullyApproved = 4,
    Rejected = 5,
    Failed = 6
}

public enum ClaimNature
{
    OpinionOnly = 1,
    InterpretationOfResearch = 2,
    CitesConcreteSources = 3
}

public enum ExpertClaimReviewStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Deferred = 4
}

public enum CredibilityTier
{
    High = 1,
    Medium = 2,
    Low = 3,
    Practitioner = 4
}

public enum ExpertPlatform
{
    YouTube = 1,
    Article = 2,
    Podcast = 3,
    Book = 4
}
