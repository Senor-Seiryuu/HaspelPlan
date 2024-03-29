# HaspelPlan

This Project is designed to cache and show a school-timetable upon startup.
There is a selection of several School-Classes and calendar week.

# [1.0.12] - 05.08.2023
### Fixed
- Fixed the timetables of all classes

# [1.0.11] - 10.02.2023
### Fixed
- Fixed the timetables of Classes ITO and ITU

# [1.0.10] - 17.11.2022
### Modified
- Renamed Variables
- Removed redundant loading of webpage
- Showing weekdays in calendarWeek-selector
- Fixed line endings to windows

### Fixed
- Deletion of unneeded timetable rows

# [1.0.9] - 03.11.2022
### Modified
- Initializes the class frames on first startup and caches the value.
- Checks on every startup, if the class frames are still valid.

# [1.0.8] - 19.08.2022
### Modified
- Changed font color of Dropdown and Update-Button

# [1.0.7] - 19.08.2022
### Fixed
- Fixed Tables to matching Frames

# [1.0.6] - 11.08.2022
### Fixed
- Fixed Tables to matching Frames

# [1.0.5] - 04.07.2022
### Modified
- Changed layout of all elements

# [1.0.4] - 03.07.2022
### Added
- Set default school-class on first startup

### Modified
- Code optimizations


# [1.0.3] - 03.07.2022
### Added
- Added Dropdown for selection of School-Class
- Saves selected School-Class in a file and selects it automatically again after every startup
- Added Dropdown for selection of calendar week
- Changed Application-architecture ---> Application is now compliant with the MVVM-Pattern

# [1.0.1] - 24.03.2022
### Added
- Added Button for refreshing the timetable without restarting the app.

### Modified
- Removed unnecessary rows and colums from timetable
- Added hours of subjects
- Updated app icon


# [1.0.0] - 15.10.2021
### Added
- Loads a specific timetable in WebView
- Caches the timetable in local storage of device
