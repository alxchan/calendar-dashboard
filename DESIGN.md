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

## 2. Design Decisions

### Objective  
Implement secure and flexible user sign-in using Google OAuth 2.0, with support for offline features and multi-session continuity.

### Options Considered

#### Option 1: Store OAuth tokens in secure cookies only  
**Pros:**
- Simple to implement.
- No need for server-side storage.
- Can handle token expiration by checking cookie contents.

**Cons:**
- Users cannot schedule tasks while offline (requires active session).
- Tokens are stored client-side, and although protected by `HttpOnly` and `Secure` flags, they could be vulnerable if not properly managed.
- Session ends when the cookie expires or is cleared.

**Behavior:**
- Access and refresh tokens are stored in the browser.
- Logout clears the cookie and ends the session.
- Token expiration is handled by checking and refreshing via cookie when needed.

#### Option 2: Store tokens in encrypted form in the database  
**Pros:**
- Enables offline task scheduling and multi-session support.
- Central control over user sessions.
- Easier to revoke access or manage long-lived sessions.

**Cons:**
- More complex implementation.
- Requires encryption/decryption logic.
- Storage needs grow with user count.
- Handling multiple device sessions could be challenging due to multiple access tokens.

**Behavior:**
- Refresh tokens are stored encrypted; access tokens may be stored temporarily or re-generated as needed.
- Sessions managed via a `signed_in` flag in the database.
- Logout flags the user as signed out but retains encrypted tokens for future re-authentication if necessary.

#### Option 3: Hybrid – Store refresh token in database and access token in secure cookie  
**Pros:**
- Combines the flexibility of database storage with the simplicity of cookies.
- Supports offline features and multi-device sessions.
- Reduces need to store short-lived access tokens in the database.

**Cons:**
- Slightly more complex to coordinate between cookie and DB logic.

**Behavior:**
- On login, refresh token is stored encrypted in the database.
- Access token is stored in a secure cookie.
- When cookie expires, access token can be reissued using the refresh token.
- Logout deletes the cookie and optionally marks the session inactive in the database.

### Selected Approach  
**Option 3: Hybrid storage** was chosen as it best supports the planned features: offline task scheduling, user session management, and eventual support for multiple devices. It balances flexibility, security, and maintainability. Access tokens will be short-lived and stored in cookies; refresh tokens will be stored securely in the database.

---

### Objective:  
Minimize latency when fetching Google Calendar data to improve user experience, while balancing complexity and resource usage.

### Options

1. **Fetch directly from the Google Calendar API on every request**  
   **Pros:**  
   - Simple to implement  
   - No additional memory or storage requirements  
   **Cons:**  
   - High latency visible to users  
   - Increased API usage, which may hit rate limits  

2. **Implement caching to check locally before calling the API**  
   **Pros:**  
   - Faster response time for repeated requests  
   - Reduces API calls, conserving quota  
   - Cache size and expiration can be adjusted as needed  
   **Cons:**  
   - More complex to implement and maintain  
   - Cached data might be slightly stale  

3. **Prefetch calendar data proactively (e.g., background tasks or on login)**  
   **Pros:**  
   - Minimal latency when user accesses calendar  
   - Can significantly improve user experience  
   **Cons:**  
   - Difficult to predict exactly when data is needed  
   - Requires complex scheduling or background processing logic  

### Selected Approach (TBD)

- The chosen approach will primarily balance latency and implementation complexity, with memory overhead as a secondary consideration.

---

### Objective
Implement a user login option that maximizes security while minimizing the number of logins required during a session.

### Options

1. **Login sessions determined by a cookie**  
   **Pros:**  
   - User sessions have a fixed length.  
   - Logging out is straightforward—just delete the cookie.  
   **Cons:**  
   - Sessions might be too short, causing users to log in multiple times in one session.

2. **Refresh login using the refresh token**  
   **Pros:**  
   - Users stay logged in indefinitely during a session.  
   **Cons:**  
   - Users need to manually log out, which is a security risk on shared devices.  
   - Adds complexity by interrupting the normal authentication flow.

### Selected Approach

Option 1 is chosen because it offers a simple implementation while reasonably protecting user credentials. Using a 30-minute sliding expiration window helps reduce frequent logins within a session.
  
---

## 3. Roadmap

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

## 4. Future Considerations

- Add support for other calendar providers (e.g., Outlook).
- Implement background sync jobs.
- Integrate dashboard charts or visualizations for task progress.

---
