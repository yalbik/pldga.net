namespace PLDGA.Domain.Entities;

public class SiteSettings
{
    // Membership & Dues
    public decimal AnnualDuesAmount { get; set; } = 30.00m;
    public DateTime? DuesPaymentDeadline { get; set; }
    public decimal LateFee { get; set; }
    public int GracePeriodDays { get; set; }

    // Event Settings
    public int DefaultRegistrationDeadlineDays { get; set; } = 3;
    public int DefaultMaxParticipants { get; set; } = 72;
    public Dictionary<int, int> PlacementPoints { get; set; } = new()
    {
        { 1, 100 }, { 2, 90 }, { 3, 82 }, { 4, 75 }, { 5, 69 },
        { 6, 64 }, { 7, 60 }, { 8, 57 }, { 9, 54 }, { 10, 51 },
        { 11, 49 }, { 12, 47 }, { 13, 45 }, { 14, 43 }, { 15, 41 },
        { 16, 39 }, { 17, 37 }, { 18, 35 }, { 19, 33 }, { 20, 31 }
    };

    // Leaderboard
    public int TopPlayersOnHomePage { get; set; } = 10;
    public bool ShowRanks { get; set; } = true;
    public bool ShowPoints { get; set; } = true;
    public bool ShowEventsPlayed { get; set; } = true;

    // Polls
    public int MaxPollAnswers { get; set; } = 10;
    public int DefaultPollDurationDays { get; set; } = 7;

    // News
    public int MaxImageSizeMb { get; set; } = 5;
    public string AllowedImageFormats { get; set; } = "jpg,jpeg,png,gif,webp";
    public int ArticlesOnHomePage { get; set; } = 6;

    // Appearance
    public string SiteTitle { get; set; } = "Parkland Disc Golf Association";
    public string SiteTagline { get; set; } = "Disc Golf in the Puget Sound";
    public string LogoPath { get; set; } = "/images/pldga.jpg";
    public string PrimaryColor { get; set; } = "#4A8FC2";
    public string SecondaryColor { get; set; } = "#D4792A";
    public string FooterText { get; set; } = "© Parkland Disc Golf Association";
    public string FacebookUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;

    // Season
    public int CurrentSeasonYear { get; set; } = DateTime.UtcNow.Year;

    // General
    public bool MaintenanceMode { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string OrganizationAddress { get; set; } = "Parkland, WA";

    // Home Page Sections
    public List<HomePageSection> HomePageSections { get; set; } = new()
    {
        new() { Key = "top_players", Title = "Top Players", Enabled = true, Order = 1 },
        new() { Key = "news", Title = "Latest News", Enabled = true, Order = 2 },
        new() { Key = "events", Title = "Upcoming Events", Enabled = true, Order = 3 },
        new() { Key = "polls", Title = "Active Polls", Enabled = true, Order = 4 }
    };
}

public class HomePageSection
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int Order { get; set; }
}
