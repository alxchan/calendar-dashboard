# Integrated Calendar Dashboard – REQUIREMENTS.md

## 1. Purpose
Integrated Calendar Dashboard is a calendar management tool which streamlines scheduling, task management and collaboration. It supports role based event scheduling, progress tracking through tasks and seamless data export for use with Excel and VBA-based workflows.

## 2. Business Requirements
- The system should provide support for synchronization between the dashboard and the calendar apps
- The user should control when synchronization occurs
- The user should be able to create events
- The user should be able to create tasks
- The system should showcase task progress clearly
- The system should support exporting tasks and events data to Excel

## 3. Functional Requirements
- Events should include the following
    - Name of the event
    - Place of the event
    - Start time
    - End time
    - Description (optional)
    - Notes (optional)
- Event should be visually distinct (e.g coloured different) if the user has not confirmed attendance
- Event should allow overlapping time periods
- Events should either be restricted (viewable only by certain roles/users) or viewable by everyone
- Events should be exportable to Excel through VBA
- Users should be able to create an event and share it
- Tasks should include the following
    - Name of the task
    - Completion rate
    - Whether the task is completed or not
    - Description (optional)
    - Notes (optional)
- A progress bar should be displayed for each task to indicate progress.
- Tasks should be exportable to Excel through VBA

## 4. Non-Functional Requirements
- The system should enforce a minimum interval of 10 seconds between consecutive manual synchronization actions
- Synchronization should complete within 1 minute, except under exceptional circumstances
- The UI should be responsive and function well on both mobile and desktop devices
- The system should provide meaningful error messages for synchronization or data failures
- The system should be accessible and comply with basic accessibility guidelines

## 5. Assumptions & Constraints
- Google Calendar API used as data source