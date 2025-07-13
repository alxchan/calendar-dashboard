# Integrated Calendar Dashboard – DESIGN.md

## 1. Technical Design

### Architecture
- The application follows the **Model-View-Controller (MVC)** pattern.
- **Reasoning:** While MVC introduces structure and may add complexity early on, it allows for better separation of concerns and long-term scalability. I’m also familiar with MVVM, which makes this transition natural.

### Authentication
- **OAuth 2.0 via Google** will be used for user authentication and authorization.
- This enables secure sign-in and access to Google Calendar data required by the application.

### Data Storage
- A **SQL database** will be used for storing events, tasks, and user-related data.
- **Reasoning:** SQL offers relational structure, ease of querying, and flexible schema evolution.

### Frontend
- Views will use Razor Pages with support for responsive design (desktop + mobile).

### API Integration
- The application will connect to the **Google Calendar API** to synchronize events.
- Synchronization will be user-initiated and respect limits (e.g., cooldown of 10 seconds).
- Event data will be pulled from or pushed to Google Calendar based on user actions. Data may be displayed in the UI or stored locally depending on context.

---

## 2. Roadmap

The project will be built in stages, roughly following this order:

1. Define requirements
2. Set up project structure (MVC, database, Git)
3. Implement OAuth 2.0 authentication
4. Connect and test Google Calendar API
5. Build event and task management features
6. Implement export to Excel functionality
7. Improve UI and accessibility
8. Testing and error handling
9. Deployment/readiness for production

---

## 3. Future Considerations

- Add support for other calendar providers (e.g., Outlook).
- Implement background sync jobs.
- Integrate dashboard charts or visualizations for task progress.

---