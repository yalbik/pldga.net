# Parkland Disc Golf Association (PLDGA) Website - Project Specification

## Project Overview

Create a comprehensive .NET/C# web application for the Parkland Disc Golf Association (PLDGA) located in Parkland, WA. This application will manage members, events, leaderboards, polls, and news articles for the disc golf community in the Puget Sound area.

## Architecture Requirements

### Clean Architecture Pattern
Implement Clean Architecture with the following layers:

1. **Domain Layer** (Core)
   - Contains business entities and business rules
   - Interfaces for repositories and services
   - Value objects and domain-specific types
   - No dependencies on outer layers

2. **Application Layer**
   - Application services coordinating use cases
   - DTOs (Data Transfer Objects)
   - Interfaces for external concerns
   - Validation logic

3. **Infrastructure Layer**
   - JSON file-based data persistence implementation
   - File system operations
   - External service implementations
   - Dependency injection setup

4. **Presentation Layer**
   - ASP.NET Core MVC or Razor Pages
   - Controllers or Page Models
   - Views with modern, responsive UI
   - ViewModels
   - Static assets (CSS, JavaScript, images)

### Project Structure
Organize the solution with separate projects for each architectural layer:
- PLDGA.Domain
- PLDGA.Application
- PLDGA.Infrastructure
- PLDGA.Web (Presentation)
- PLDGA.Tests (Unit tests)

## Core Entities and Data Models

### Member Entity
Members represent individuals in the PLDGA system with the following characteristics:

- Unique identifier
- Personal information (name, email, contact details)
- Membership status (Paid or Unpaid)
- Membership type (Admin or Regular User)
- Registration date
- Total accumulated points for current season
- Payment status tracking
- Unpaid members must be clearly labeled whenever displayed anywhere in the UI

### Event Entity
Monthly disc golf competitions held at various Puget Sound locations:

- Unique identifier
- Event name/title
- Date and time
- Location details (name, address, coordinates if available)
- Course information
- Maximum participants
- Registration deadline
- Event status (upcoming, active, completed)
- Results data linking participants to their standings
- Points awarded based on placement

### Poll Entity
Community voting mechanism:

- Unique identifier
- Question text
- Multiple answer options (maximum 10 answers)
- Creation date
- Created by (user reference)
- Status (active, closed)
- Vote counts per answer
- Optional: end date for automatic closure

### News Article Entity
Community news and announcements:

- Unique identifier
- Title
- Content/body text
- Author (user reference)
- Creation date
- Last modified date
- Approval status (pending, approved, rejected)
- Associated images (file paths or embedded data)
- Featured flag for main page display

### Season Entity
Annual competition tracking:

- Year
- Start date (January 1st)
- End date (December 31st)
- Current status
- Associated events

## Functional Requirements

### Authentication and Authorization

#### User Roles
1. **Admin Users**
   - Can create and manage events
   - Can add and manage users/players
   - Can approve or reject news articles
   - Can edit main page structure
   - Can manage polls
   - Full system access

2. **Regular Users (Paid Members)**
   - Can view events and register
   - Can view leaderboards and standings
   - Can create polls
   - Can create news articles (pending approval)
   - Can vote on polls
   - Can view their personal statistics

3. **Unpaid Members**
   - Can compete in events
   - Do NOT earn points
   - Must be labeled as "Unpaid" in all displays
   - Can view public information
   - Limited participation features

#### Security Requirements
- Implement secure authentication (ASP.NET Core Identity or custom)
- Password hashing and secure storage
- Role-based authorization
- Protect admin endpoints
- Validate user permissions on all operations
- Implement CSRF protection
- Use HTTPS in production

### Member Management

#### Admin Features
- Create new member accounts
- Edit member information
- **Quick-Pay Interface**: Easy one-click or bulk interface to mark users as "Paid" when they pay dues
  - Toggle switch or button to change payment status instantly
  - Bulk payment processing for multiple members at once
  - Payment date tracking and history
  - Visual indicators for paid vs unpaid status in member lists
- Set membership status (Paid/Unpaid)
- Assign admin privileges
- View all members
- Search and filter members
- Filter by payment status
- Export member lists

#### Member Display
- Always show "Unpaid" label for unpaid members
- Display membership type clearly
- Show current season points
- Display member statistics
- Show payment status and dues payment date

### Event Management

#### Event Creation (Admin Only)
- Create new monthly events
- Set event details (date, location, capacity)
- Configure registration parameters
- Manage event status

#### Event Participation
- Members can register for events
- Track participant standings
- Record event results
- Calculate and award points to paid members
- Unpaid members participate but earn zero points

#### Event Display
- List upcoming events
- Show event details
- Display participant lists
- Show completed event results
- Link to historical events

### Points and Leaderboard System

#### Points Calculation
- Points awarded based on event standing/placement
- Only paid members accumulate points
- Points are season-specific (reset annually)
- Track cumulative season points

#### Leaderboard Features
- Display current season standings
- Show top players prominently
- Display individual player rankings
- Filter and search capabilities
- Historical season data access
- Drill-down to individual event details

#### Season Standings Display
- Season-to-date totals
- Individual event breakdowns
- Player statistics
- Visual ranking display
- Export capabilities (optional)

### Poll System

#### Poll Creation
- Any user can create polls (including non-admins)
- Enter poll question
- Add multiple answers (1-10 maximum)
- Set poll duration (optional)
- Submit for publication

#### Poll Voting
- View active polls
- Vote on polls (one vote per user per poll)
- View poll results
- Display vote counts and percentages

#### Poll Display on Front Page
- Link to polls section on main page
- Display poll titles with links
- Show active polls prominently

### News Article System

#### Article Creation
- Any player can create news articles
- Rich text content support
- Image upload capability
- Multiple image support
- Author attribution
- Submit for approval

#### Article Approval Workflow
- Articles submitted by players are pending approval
- Admins review pending articles
- Admins can approve or reject articles
- Approved articles appear on main page
- Rejected articles are hidden

#### News Display
- Main page shows approved article thumbnails
- Dedicated news page with all articles
- Filter by date, author, status
- Click thumbnails to read full articles

### Main Page (Home Page)

#### Requirements
- Feature-rich and visually engaging design
- Modern, responsive layout
- Attention-grabbing elements

#### Content Sections
1. **Top 10 Players Section**
   - Prominently display current top 10 players
   - Show player names and points
   - Direct link to full leaderboard page
   - Real-time or frequently updated data

2. **News Articles Section**
   - Display thumbnails of admin-approved articles
   - Show article titles
   - Clickable links to full articles
   - Recent articles prioritized

3. **Upcoming Events**
   - Show next scheduled event
   - Event date and location
   - Registration link

4. **Active Polls**
   - Link to polls section
   - Optionally show current poll question

#### Admin Customization
- Admin page for editing main page structure
- Reorder sections
- Enable/disable sections
- Configure display settings
- Set featured content

### Site Settings Page (Admin Only)

#### Comprehensive Configuration Interface
Admins should have access to a centralized settings page to configure all site-wide parameters:

#### Membership & Dues Settings
- **Annual Dues Amount**: Configurable dollar amount (default: $30/year)
- Dues payment deadline date (optional)
- Late fee configuration (optional)
- Grace period settings (optional)
- Membership renewal reminder settings

#### Event Settings
- Default event registration deadline (days before event)
- Maximum participants per event (default value)
- Points configuration for event placements
- Event notification settings
- Default course information templates

#### Leaderboard Settings
- Number of top players to display on home page (default: 10)
- Points calculation method configuration
- Tie-breaker rules
- Display options (show ranks, show points, show events played)

#### Poll Settings
- Maximum answers per poll (default: 10)
- Default poll duration (days)
- Voting restrictions (one vote per user, etc.)
- Poll visibility settings

#### News Settings
- Maximum image size for articles (MB)
- Allowed image formats
- Articles to display on home page (count)
- Auto-approval settings for trusted users (optional)

#### Site Appearance Settings
- Site title and tagline
- Logo upload and configuration
- Color scheme customization
- Homepage layout preferences
- Footer text and links
- Social media links

#### Notification Settings
- Email notification preferences
- Event reminder timing
- Dues payment reminder timing
- Admin alert configurations

#### Security Settings
- Password policy configuration
- Session timeout settings
- Login attempt limits
- Two-factor authentication options (optional)

#### Data & Backup Settings
- Automatic backup frequency
- Backup retention period
- Data export options
- JSON file management

#### Season Settings
- Current season year
- Season start and end date overrides (if not Jan 1 - Dec 31)
- Season transition automation

#### General Settings
- Site maintenance mode toggle
- Contact information
- Organization address
- Phone number
- Email address

## Data Persistence

### JSON File Storage
- Use JSON files for all data persistence
- One JSON file per entity type:
  - members.json
  - events.json
  - polls.json
  - news_articles.json
  - seasons.json
  - user_accounts.json (for authentication)
  - site_settings.json (for all site configuration)

### Data Persistence Layer Design
- Repository pattern implementation
- File-based repository for each entity
- Thread-safe file operations
- Proper error handling
- Data validation on save
- Backup considerations
- Atomic write operations (write to temp, then rename)

### Repository Interfaces
Define interfaces in Domain layer:
- IMemberRepository
- IEventRepository
- IPollRepository
- INewsArticleRepository
- ISeasonRepository
- IUserRepository

### Implementation Requirements
- Implement repositories in Infrastructure layer
- Use file locking to prevent concurrent write issues
- Implement CRUD operations
- Support querying and filtering
- Handle file creation if not exists
- Graceful error handling for corrupted files

## User Interface Requirements

### Design Principles
- Modern, clean aesthetic
- Responsive design (mobile, tablet, desktop)
- Consistent navigation
- Accessible (WCAG compliance where possible)
- Fast loading times
- Intuitive user experience

### Key Pages

1. **Home Page** (see Main Page section above)

2. **Leaderboard Page**
   - Full season standings
   - Sortable columns
   - Search functionality
   - Filter options
   - Event drill-down capability

3. **Events Page**
   - List of all events
   - Upcoming events section
   - Past events section
   - Event details modal or separate page
   - Registration interface

4. **Member Profile Page**
   - Personal statistics
   - Event participation history
   - Current standing
   - Points breakdown

5. **Polls Page**
   - Active polls list
   - Create poll interface
   - Vote interface
   - Results display

6. **News Page**
   - All approved articles
   - Create article interface
   - Article detail view
   - Image gallery support

7. **Admin Dashboard**
   - Member management
   - Quick payment processing interface
   - Event management
   - News approval queue
   - Main page configuration
   - System statistics
   - Site settings access

8. **Site Settings Page** (Admin Only)
   - All configuration options organized by category
   - Easy-to-use forms for all settings
   - Save and cancel functionality
   - Validation for all inputs
   - Confirmation messages
   - Settings categories with navigation

9. **Authentication Pages**
   - Login
   - Register (if self-registration enabled)
   - Password reset

## Testing Requirements

### Unit Testing Framework
- Use NUnit for all unit tests
- Target minimum 80% code coverage
- Comprehensive test suite

### Test Categories

1. **Domain Tests**
   - Entity validation
   - Business rule enforcement
   - Value object behavior
   - Domain logic

2. **Application Tests**
   - Service layer logic
   - Use case implementation
   - DTO transformations
   - Validation pipelines

3. **Infrastructure Tests**
   - Repository operations
   - File I/O operations
   - Data persistence correctness
   - Error handling

4. **Integration Tests** (Optional but recommended)
   - End-to-end workflows
   - API endpoint testing
   - Authentication flows

### Test Coverage Areas
- All business logic
- Points calculation algorithms
- Event management workflows
- Poll creation and voting
- News article approval process
- User authorization checks
- Data validation
- Edge cases and error conditions

### Code Coverage
- Use a coverage tool (e.g., Coverlet)
- Generate coverage reports
- Enforce 80% minimum threshold
- Include coverage in build process

## Build and Deployment

### Build Requirements
- .NET 8.0 or later
- Successful build with no errors
- All tests must pass
- Code coverage report generation

### Build Process
1. Restore NuGet packages
2. Build all projects
3. Run all unit tests
4. Generate code coverage report
5. Verify coverage meets 80% threshold
6. Publish application if all checks pass

### Docker Containerization
- Include a Dockerfile in the project root
- Use official .NET multi-stage build for optimal image size
- Stage 1: Build stage using mcr.microsoft.com/dotnet/sdk:8.0
- Stage 2: Runtime stage using mcr.microsoft.com/dotnet/aspnet:8.0
- Expose port 10420 for external access
- Copy published application to container
- Set proper working directory
- Configure entry point to run the application
- Ensure JSON data files are properly mounted or included
- Support for volume mounting for data persistence
- Health check configuration
- Proper environment variable handling

### Docker Requirements
- Container should listen on port 10420
- Include .dockerignore file to exclude unnecessary files
- Optimize layer caching for faster builds
- Use non-root user for security (optional but recommended)
- Include documentation for Docker build and run commands in README.md

### Project Configuration
- Use .NET SDK-style projects
- Configure dependency injection
- Set up proper logging
- Configure app settings
- Environment-specific configurations

## Documentation

### README.md Requirements
Create comprehensive README.md including:

1. **Project Title and Description**
   - Parkland Disc Golf Association Website
   - Brief overview of functionality

2. **Features Summary**
   - Member management with paid/unpaid tracking
   - Event tracking and management
   - Season leaderboards with drill-down
   - Community polls
   - News articles with approval workflow
   - Admin capabilities and site settings
   - Annual dues management ($30/year default)

3. **Technology Stack**
   - .NET 8.0 or later
   - ASP.NET Core
   - NUnit for testing
   - Clean Architecture pattern
   - Docker containerization

4. **Project Structure**
   - Solution layout
   - Layer descriptions (Domain, Application, Infrastructure, Web, Tests)

5. **Prerequisites**
   - .NET 8.0 SDK or later
   - Docker and Docker Compose (optional for containerized deployment)
   - Modern web browser

6. **Building the Project**
   - Prerequisites installation
   - Build commands (dotnet build)
   - Test execution (dotnet test)
   - Code coverage generation

7. **Running the Application**
   - Development run instructions (dotnet run)
   - Configuration requirements
   - Default URL and port information

8. **Docker Deployment**
   - Building the Docker image: `docker build -t pldga-app .`
   - Running the container: `docker run -p 10420:10420 pldga-app`
   - Volume mounting for data persistence
   - Environment variable configuration
   - Docker Compose example (optional)

9. **Running Tests**
   - Test execution commands (dotnet test)
   - Coverage report generation
   - Minimum 80% coverage requirement

10. **Data Files**
    - Description of JSON storage system
    - List of JSON files (members.json, events.json, etc.)
    - Initial setup requirements
    - Backup recommendations

11. **Admin Features**
    - How to access admin dashboard
    - Managing member payment status
    - Configuring annual dues
    - Site settings overview

12. **Configuration**
    - App settings explanation
    - Environment-specific settings
    - Default values (including $30 annual dues)

13. **Troubleshooting**
    - Common issues and solutions
    - Log file locations
    - Support contact information

14. **License**
    - GPL3

15. **Contributing** 
    - Submit a pull request

## Additional Considerations

### Performance
- Optimize file I/O operations
- Implement caching where appropriate
- Minimize database-like queries on JSON files
- Consider file size limits and pagination

### Security
- Validate all user input
- Sanitize content (especially news articles)
- Implement rate limiting for polls/voting
- Secure file access
- Prevent unauthorized file modifications

### Error Handling
- Graceful error messages
- Logging of errors
- User-friendly error pages
- Admin notifications for critical errors

### Future Extensibility
- Design for potential migration to real database
- Plugin architecture for features
- API-ready design
- Configuration-driven behavior

## Success Criteria

The project is considered complete when:

1. All functional requirements are implemented
2. Clean Architecture is properly followed
3. JSON file persistence is working correctly
4. All unit tests pass
5. Code coverage is at least 80%
6. Authentication and authorization work correctly
7. All user roles have appropriate access
8. Main page is feature-rich and visually appealing
9. Leaderboard displays correctly with drill-down
10. Poll system functions as specified
11. News article workflow operates correctly
12. Admin features are fully functional
13. Site settings page is comprehensive and functional
14. Annual dues configuration works (default $30/year)
15. Quick payment interface for marking users as paid is easy to use
16. Dockerfile builds successfully and container runs on port 10420
17. README.md is comprehensive with Docker instructions
18. Build process completes successfully
19. Application runs without errors
20. Docker container can be built and run successfully

## Implementation Notes for the LLM

- Focus on clean, maintainable code
- Follow C# best practices and conventions
- Use async/await appropriately
- Implement proper dependency injection
- Write meaningful commit messages if using version control
- Comment complex business logic
- Create comprehensive XML documentation for public APIs
- Ensure the UI is modern and professional
- Test edge cases thoroughly
- Make the application production-ready
- Create a proper Dockerfile with multi-stage build
- Ensure the application listens on the correct port in Docker (10420)
- Implement intuitive admin interface for payment processing
- Make site settings page user-friendly and well-organized
